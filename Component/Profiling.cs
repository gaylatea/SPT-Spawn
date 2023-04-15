using System;
using System.Data.SQLite;
using System.Diagnostics;
using System.Reflection;

using HarmonyLib;

using UnityEngine;

using Aki.Reflection.Utils;

using static Config.Profiles;

namespace Framesaver
{
    public class Profiling : MonoBehaviour
    {
        private static string currentLocalGame;

        private static Harmony _harmony;
        private static HarmonyMethod methodPrefix;
        private static HarmonyMethod methodPostfix;

        private static string queryCreateFlamegraphTable = @"CREATE TABLE IF NOT EXISTS calls (
            frame number,
            game text,
            component text,
            phase text,
            time_ms number
        );";

        private static string queryAddFlamegraphCall = @"INSERT INTO calls(
            frame, game, component, phase, time_ms
        ) VALUES (
            $frame, $game, $component, $phase, $time_ms
        );";
        private static SQLiteConnection db = new SQLiteConnection("Data Source=flamegraph.sqlite3;Version=3;New=True;Compress=True;");

        public void Awake()
        {
            _harmony = new Harmony("Framesaver-Profiling");

            db.Open();

            var create_cmd = db.CreateCommand();
            create_cmd.CommandText = queryCreateFlamegraphTable;
            create_cmd.ExecuteNonQuery();

            methodPrefix = new HarmonyMethod(typeof(Profiling).GetMethod("Prefix", BindingFlags.Static | BindingFlags.Public));
            methodPostfix = new HarmonyMethod(typeof(Profiling).GetMethod("Postfix", BindingFlags.Static | BindingFlags.Public));

            var prefixActivate = new HarmonyMethod(typeof(Profiling).GetMethod("RecordGameDetails", BindingFlags.Static | BindingFlags.Public));
            _harmony.Patch(PatchConstants.LocalGameType.GetMethod("vmethod_0", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public), prefix: prefixActivate);
        }

        // This patch will add a marker for the current game to any profiling
        // calls that are made, so that multiple runs can be analyzed easily.
        public static bool RecordGameDetails(object __instance)
        {
            currentLocalGame = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");

            return true;
        }

        public void EnableOn(Type typeName, string methodName)
        {
            var m = typeName.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (m != null)
            {
                _harmony.Patch(m, prefix: methodPrefix, postfix: methodPostfix);
            }
        }

        public static bool Prefix(ref Stopwatch __state)
        {
            if(!Enabled.Value) { return true; }

            if (__state == null) { __state = new Stopwatch(); }
            __state.Start();
            return true;
        }

        public static void Postfix(ref Stopwatch __state, MethodBase __originalMethod)
        {
            if(!Enabled.Value) { return; }

            __state.Stop();

            var cmd = db.CreateCommand();
            cmd.CommandText = queryAddFlamegraphCall;
            cmd.Parameters.AddWithValue("$frame", Time.frameCount);
            cmd.Parameters.AddWithValue("$game", currentLocalGame);
            cmd.Parameters.AddWithValue("$component", __originalMethod.DeclaringType.Name);
            cmd.Parameters.AddWithValue("$phase", __originalMethod.Name);
            cmd.Parameters.AddWithValue("$time_ms", __state.Elapsed.TotalMilliseconds);
            cmd.ExecuteNonQuery();

            __state.Reset();
        }
    }
}