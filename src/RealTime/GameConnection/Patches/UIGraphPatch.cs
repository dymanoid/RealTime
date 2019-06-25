// <copyright file="UIGraphPatch.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.GameConnection.Patches
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using ColossalFramework;
    using ColossalFramework.UI;
    using SkyTools.Patching;
    using SkyTools.Tools;
    using UnityEngine;
    using static ColossalFramework.UI.UIGraph;

    /// <summary>
    /// A static class that provides the patch objects for the statistics graph.
    /// </summary>
    internal static class UIGraphPatch
    {
        private const int MinRangeInDays = 7;
        private const int MinPointsCount = 32;
        private const float GridIntervalX = 768f;

        private static CultureInfo currentCulture = CultureInfo.CurrentCulture;

        /// <summary>Gets the patch object for the minimum data points method.</summary>
        public static IPatch MinDataPoints { get; } = new UIGraph_GetMinDataPoints();

        /// <summary>Gets the patch object for the visible end time method.</summary>
        public static IPatch VisibleEndTime { get; } = new UIGraph_GetVisibleEndTime();

        /// <summary>Gets the patch object for the build labels method.</summary>
        public static IPatch BuildLabels { get; } = new UIGraph_BuildLabels();

        /// <summary>Translates the X-axis of the graph using the specified culture information.</summary>
        /// <param name="cultureInfo">The culture information to use for the X axis labels formatting.</param>
        /// <exception cref="ArgumentNullException">Thrown when the argument is null.</exception>
        public static void Translate(CultureInfo cultureInfo)
            => currentCulture = cultureInfo ?? throw new ArgumentNullException(nameof(cultureInfo));

        private static int GetMinDataPoints(List<CurveSettings> curves, DateTime startTime, DateTime endTime)
        {
            if (curves.Count >= 1 && startTime != endTime && curves[curves.Count - 1].data.Length >= 2)
            {
                long minRange = startTime.AddDays(MinRangeInDays).Ticks - startTime.Ticks;
                long ticksPerPoint = (endTime.Ticks - startTime.Ticks) / (curves[curves.Count - 1].data.Length - 1);
                int result = Mathf.CeilToInt((float)minRange / ticksPerPoint);
                return Math.Max(MinPointsCount, result);
            }

            return MinPointsCount;
        }

        private static DateTime GetVisibleEndTime(List<CurveSettings> curves, DateTime startTime, DateTime endTime)
        {
            if (curves.Count >= 1 && startTime != endTime && curves[curves.Count - 1].data.Length >= 2)
            {
                int minPointsCount = GetMinDataPoints(curves, startTime, endTime);
                int pointsCount = Math.Max(minPointsCount, curves[curves.Count - 1].data.Length);
                long ticksPerPoint = (endTime.Ticks - startTime.Ticks) / (curves[curves.Count - 1].data.Length - 1);
                return new DateTime(startTime.Ticks + (pointsCount - 1) * ticksPerPoint);
            }

            return new DateTime(1, 1, 1);
        }

        private sealed class UIGraph_GetMinDataPoints : PatchBase
        {
            protected override MethodInfo GetMethod()
            {
                return typeof(UIGraph).GetMethod(
                    "GetMinDataPoints",
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new Type[0],
                    new ParameterModifier[0]);
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Redundancy", "RCS1213", Justification = "Harmony patch")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming Rules", "SA1313", Justification = "Harmony patch")]
            private static bool Prefix(List<CurveSettings> ___m_Curves, DateTime ___m_StartTime, DateTime ___m_EndTime, ref int __result)
            {
                __result = GetMinDataPoints(___m_Curves, ___m_StartTime, ___m_EndTime);
                return false;
            }
        }

        private sealed class UIGraph_GetVisibleEndTime : PatchBase
        {
            protected override MethodInfo GetMethod()
            {
                return typeof(UIGraph).GetMethod(
                    "GetVisibleEndTime",
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new Type[0],
                    new ParameterModifier[0]);
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Redundancy", "RCS1213", Justification = "Harmony patch")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming Rules", "SA1313", Justification = "Harmony patch")]
            private static bool Prefix(List<CurveSettings> ___m_Curves, DateTime ___m_StartTime, DateTime ___m_EndTime, ref DateTime __result)
            {
                __result = GetVisibleEndTime(___m_Curves, ___m_StartTime, ___m_EndTime);
                return false;
            }
        }

        private sealed class UIGraph_BuildLabels : PatchBase
        {
            private static PixelsToUnitsDelegate pixelsToUnits;
            private static AddSolidQuadDelegate addSolidQuad;

            public UIGraph_BuildLabels()
            {
                try
                {
                    pixelsToUnits = FastDelegateFactory.Create<PixelsToUnitsDelegate>(typeof(UIComponent), "PixelsToUnits", instanceMethod: true);
                    addSolidQuad = FastDelegateFactory.Create<AddSolidQuadDelegate>(typeof(UIGraph), "AddSolidQuad", instanceMethod: true);
                }
                catch (Exception ex)
                {
                    Log.Error("The 'Real Time' mod failed to create delegates for the UIGraph. Error message: " + ex);
                }
            }

            private delegate float PixelsToUnitsDelegate(UIComponent instance);

            private delegate void AddSolidQuadDelegate(
                UIGraph instance,
                Vector2 corner1,
                Vector2 corner2,
                Color32 col,
                PoolList<Vector3> vertices,
                PoolList<int> indices,
                PoolList<Vector2> uvs,
                PoolList<Color32> colors);

            protected override MethodInfo GetMethod()
            {
                if (pixelsToUnits == null || addSolidQuad == null)
                {
                    return null;
                }
                else
                {
                    return typeof(UIGraph).GetMethod(
                        "BuildLabels",
                        BindingFlags.Instance | BindingFlags.NonPublic,
                        null,
                        new[] { typeof(PoolList<Vector3>), typeof(PoolList<int>), typeof(PoolList<Vector2>), typeof(PoolList<Color32>) },
                        new ParameterModifier[0]);
                }
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Redundancy", "RCS1213", Justification = "Harmony patch")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming Rules", "SA1313", Justification = "Harmony patch")]
            private static void Postfix(
                UIGraph __instance,
                List<CurveSettings> ___m_Curves,
                Rect ___m_GraphRect,
                PoolList<UIRenderData> ___m_RenderData,
                PoolList<Vector3> vertices,
                PoolList<int> indices,
                PoolList<Vector2> uvs,
                PoolList<Color32> colors)
            {
                // Note: this method is extracted from the original game and is slightly modified
                if (___m_Curves.Count == 0)
                {
                    return;
                }

                float units = pixelsToUnits(__instance);
                var maxSize = new Vector2(__instance.size.x, __instance.size.y);
                Vector3 center = __instance.pivot.TransformToCenter(__instance.size, __instance.arbitraryPivotOffset) * units;
                Vector3 size = units * __instance.size;
                float aspectRatio = __instance.size.x / __instance.size.y;

                using (UIFontRenderer uiFontRenderer = __instance.font.ObtainRenderer())
                {
                    float xmin = units * __instance.width * (-0.5f + ___m_GraphRect.xMin);
                    float xmax = units * __instance.width * (-0.5f + ___m_GraphRect.xMax);
                    float minInterval = GridIntervalX * units;

                    float startTicks = __instance.StartTime.Ticks;
                    DateTime visibleEndTime = GetVisibleEndTime(___m_Curves, __instance.StartTime, __instance.EndTime);
                    float endTicks = visibleEndTime.Ticks;

                    float minRangeTicks = __instance.StartTime.AddDays(MinRangeInDays).Ticks - __instance.StartTime.Ticks;
                    float rangeTicks = __instance.EndTime.Ticks - __instance.StartTime.Ticks;
                    float timeScale = minRangeTicks / rangeTicks;

                    float scaledX = Mathf.Lerp(xmin, xmax, timeScale) - xmin;
                    TimeSpan step = scaledX <= 0.0001f ? TimeSpan.FromDays(10000) : TimeSpan.FromDays(minInterval / scaledX);

                    UIRenderData textRenderData = ___m_RenderData.Count > 1 ? ___m_RenderData[1] : null;

                    for (DateTime current = __instance.StartTime.AddDays(1); current <= visibleEndTime; current += step)
                    {
                        float currentTicks = current.Ticks;
                        float x = Mathf.Lerp(xmin, xmax, (currentTicks - startTicks) / (endTicks - startTicks));
                        string text = current.ToString("M", currentCulture);
                        uiFontRenderer.textScale = 1f;
                        uiFontRenderer.vectorOffset = new Vector3(x, __instance.height * units * -0.95f, 0f);
                        uiFontRenderer.pixelRatio = units;
                        uiFontRenderer.maxSize = maxSize;
                        uiFontRenderer.textAlign = UIHorizontalAlignment.Center;
                        uiFontRenderer.defaultColor = __instance.TextColor;
                        uiFontRenderer.Render(text, textRenderData);
                        float val = Mathf.Lerp(-0.5f + ___m_GraphRect.xMin, -0.5f + ___m_GraphRect.xMax, (currentTicks - startTicks) / (endTicks - startTicks));
                        var corner1 = new Vector2(val - units * __instance.HelpAxesWidth * aspectRatio, -0.5f + ___m_GraphRect.yMin);
                        var corner2 = new Vector2(val + units * __instance.HelpAxesWidth * aspectRatio, corner1.y + ___m_GraphRect.height);
                        addSolidQuad(
                            __instance,
                            Vector3.Scale(corner1, size) + center,
                            Vector3.Scale(corner2, size) + center,
                            __instance.HelpAxesColor,
                            vertices,
                            indices,
                            uvs,
                            colors);
                    }
                }
            }
        }
    }
}