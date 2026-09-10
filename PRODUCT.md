# Product

<!-- impeccable:product-schema 1 -->

## Platform

adaptive

## Stack

Avalonia on .NET; chosen by the user.

## Users

Windows desktop users who need to save a clean screenshot of a specific application window.

## Product Purpose

Let a user select an external application window and capture it without including ScreenshotCapture itself. Success is a saved image with no audible system-capture sound.

## Positioning

The app lists eligible top-level windows, excludes its own window, and temporarily silences the Windows master endpoint while a capture is in progress before restoring the exact prior volume and mute state.

## Operating Context

The user opens ScreenshotCapture, refreshes the window list, chooses an application, and saves a PNG capture. Target applications can be minimized or temporarily unavailable.

## Capabilities and Constraints

- Capture only a user-selected top-level Windows window.
- ScreenshotCapture's own process windows are never offered as a target.
- Muting must be scoped to the capture operation and restoration must run even when capture fails.
- Output is a PNG chosen through the native save dialog.

## Evidence on Hand

No existing source code, visual assets, or product copy were supplied.

## Product Principles

- Make the selected capture target unmistakable.
- Keep the primary workflow to selection, capture, save.
- Protect the user's audio setting by always restoring it.
- Explain failure states in terms of a recovery action.
