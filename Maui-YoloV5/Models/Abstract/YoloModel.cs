﻿using MauiYoloV5;
using System.Collections.Generic;

namespace Maui_YoloV5.Models.Abstract;

/// <summary>
/// Model descriptor.
/// </summary>
public record YoloModel(
    int Width,
    int Height,
    int Depth,
    int Dimensions,
    int[] Strides,
    int[][][] Anchors,
    int[] Shapes,
    float Confidence,
    float MulConfidence,
    float Overlap,
    string[] Outputs,
    List<YoloLabel> Labels,
    bool UseDetect
);
