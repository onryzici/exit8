using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
public static class StationPolishQA {
 public static void Run(){CorridorLoopQA.Run();new GameObject("Camera effect verification").AddComponent<StationPolishProbe>();}
}
