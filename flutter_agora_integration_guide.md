# Agora Video/Audio Calling — Flutter Integration Guide

> This guide walks you through integrating Agora video/audio calling into your Flutter app using the Minicato backend API endpoints.

---

## Table of Contents

1. [Overview & Architecture](#1-overview--architecture)
2. [Required Packages](#2-required-packages)
3. [Platform Setup (Android & iOS)](#3-platform-setup)
4. [API Endpoints Reference](#4-api-endpoints-reference)
5. [Step-by-Step Implementation](#5-step-by-step-implementation)
6. [Complete Call Flow](#6-complete-call-flow)
7. [Error Handling](#7-error-handling)

---

## 1. Overview & Architecture

```
┌─────────────┐        ┌──────────────────┐        ┌─────────────┐
│  Flutter App │───────▶│  Minicato API  │───────▶│  Agora Cloud │
│  (Client)    │◀───────│  (Your Backend)   │◀───────│  (RTC Server)│
└─────────────┘        └──────────────────┘        └─────────────┘
```

**How it works:**
1. Flutter app calls your backend to get a **token** and **channel name**
2. Backend creates a `CallSession` record and returns the token
3. Flutter uses the token to join the Agora channel directly (peer-to-peer via Agora's servers)
4. When the call ends, Flutter calls your backend to update the call status

> [!IMPORTANT]
> The Flutter app **never** generates tokens itself. Tokens are always generated server-side for security. Your Agora App ID and Certificate stay on the backend only.

---

## 2. Required Packages

Add these to your `pubspec.yaml`:

```yaml
dependencies:
  # Agora Video/Voice SDK — the core engine
  agora_rtc_engine: ^6.3.2

  # Agora UI Kit — pre-built call UI (buttons, video tiles, layouts)
  agora_uikit: ^1.3.7

  # For making API calls to your backend
  dio: ^5.4.0          # or http: ^1.2.0

  # For managing permissions (camera, microphone)
  permission_handler: ^11.3.0
```

Then run:

```bash
flutter pub get
```

### What each package does

| Package | Purpose |
|---|---|
| `agora_rtc_engine` | Core Agora SDK — handles the actual video/audio streaming |
| `agora_uikit` | Pre-built UI widgets — call screen, mute/unmute buttons, camera toggle, end call button. Saves you from building the UI from scratch |
| `dio` | HTTP client to call your backend API endpoints |
| `permission_handler` | Request camera & microphone permissions at runtime |

---

## 3. Platform Setup

### Android

In `android/app/build.gradle`:

```gradle
android {
    defaultConfig {
        minSdkVersion 24  // Agora requires minimum SDK 24
    }
}
```

In `android/app/src/main/AndroidManifest.xml`, add these permissions:

```xml
<manifest>
    <!-- Agora permissions -->
    <uses-permission android:name="android.permission.INTERNET" />
    <uses-permission android:name="android.permission.CAMERA" />
    <uses-permission android:name="android.permission.RECORD_AUDIO" />
    <uses-permission android:name="android.permission.MODIFY_AUDIO_SETTINGS" />
    <uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />
    <uses-permission android:name="android.permission.BLUETOOTH" />
    <uses-permission android:name="android.permission.ACCESS_WIFI_STATE" />
    
    <application>
        <!-- ... -->
    </application>
</manifest>
```

### iOS

In `ios/Runner/Info.plist`, add:

```xml
<key>NSCameraUsageDescription</key>
<string>We need camera access for video calls</string>
<key>NSMicrophoneUsageDescription</key>
<string>We need microphone access for calls</string>
```

In `ios/Podfile`, set the minimum iOS version:

```ruby
platform :ios, '13.0'
```

Then run:

```bash
cd ios && pod install && cd ..
```

---

## 4. API Endpoints Reference

### 4.1 Generate Call Token

Creates a new call session (or returns an existing active one for reconnection).

```
POST /api/chats/{chatId}/call
Authorization: Bearer <your_jwt_token>
Content-Type: application/json
```

**Request Body:**
```json
{
  "callType": 0
}
```

| Value | Meaning |
|---|---|
| `0` | Video Call |
| `1` | Voice Call |

**Success Response (200):**
```json
{
  "isSuccess": true,
  "value": {
    "token": "007eJxTYBBY...",
    "channelName": "chat_42_a1b2c3d4e5f6...",
    "callSessionId": 15,
    "agoraUid": 5
  }
}
```

| Field | Description |
|---|---|
| `token` | Agora RTC token — pass this to Agora SDK to join the channel |
| `channelName` | Unique channel name — both caller and receiver must join the same channel |
| `callSessionId` | ID of the call record — use this to update call status later |
| `agoraUid` | Your user ID cast as uint — Agora uses this to identify you in the channel |

### 4.2 Update Call Status

Updates the lifecycle status of an existing call.

```
PUT /api/chats/calls/{callSessionId}/status
Authorization: Bearer <your_jwt_token>
Content-Type: application/json
```

**Request Body:**
```json
{
  "status": 2
}
```

| Value | Meaning | When to send |
|---|---|---|
| `1` | Ongoing | Receiver picked up the call |
| `2` | Completed | Either side hung up normally |
| `3` | Missed | Receiver didn't answer (timeout) |
| `4` | Rejected | Receiver declined the call |

**Success Response (200):**
```json
{
  "isSuccess": true,
  "value": "Call status updated to 'Completed'."
}
```

---

## 5. Step-by-Step Implementation

### Step 1: Create the API Service

```dart
// lib/services/call_api_service.dart

import 'package:dio/dio.dart';

class CallApiService {
  final Dio _dio;

  CallApiService(this._dio);

  /// Calls the backend to get an Agora token and channel name.
  /// [chatId] — the ID of the chat between the two users.
  /// [callType] — 0 for Video, 1 for Voice.
  Future<CallTokenResponse> generateCallToken(int chatId, int callType) async {
    final response = await _dio.post(
      '/api/chats/$chatId/call',
      data: {'callType': callType},
    );

    final value = response.data['value'];
    return CallTokenResponse(
      token: value['token'],
      channelName: value['channelName'],
      callSessionId: value['callSessionId'],
      agoraUid: value['agoraUid'],
    );
  }

  /// Updates the call status on the backend.
  /// [callSessionId] — returned from generateCallToken.
  /// [status] — 1=Ongoing, 2=Completed, 3=Missed, 4=Rejected.
  Future<void> updateCallStatus(int callSessionId, int status) async {
    await _dio.put(
      '/api/chats/calls/$callSessionId/status',
      data: {'status': status},
    );
  }
}

class CallTokenResponse {
  final String token;
  final String channelName;
  final int callSessionId;
  final int agoraUid;

  CallTokenResponse({
    required this.token,
    required this.channelName,
    required this.callSessionId,
    required this.agoraUid,
  });
}
```

### Step 2: Request Permissions

```dart
// lib/utils/permissions.dart

import 'package:permission_handler/permission_handler.dart';

/// Requests camera and microphone permissions.
/// Returns true if both are granted.
Future<bool> requestCallPermissions({required bool isVideo}) async {
  // Always need microphone for calls
  final micStatus = await Permission.microphone.request();

  if (isVideo) {
    final cameraStatus = await Permission.camera.request();
    return micStatus.isGranted && cameraStatus.isGranted;
  }

  return micStatus.isGranted;
}
```

### Step 3: Build the Call Screen (Using Agora UI Kit)

This is the easiest approach — `agora_uikit` gives you a ready-made call UI:

```dart
// lib/screens/call_screen.dart

import 'package:agora_uikit/agora_uikit.dart';
import 'package:flutter/material.dart';
import '../services/call_api_service.dart';

class CallScreen extends StatefulWidget {
  final String token;
  final String channelName;
  final int agoraUid;
  final int callSessionId;
  final bool isVideo;
  final CallApiService callApiService;

  const CallScreen({
    super.key,
    required this.token,
    required this.channelName,
    required this.agoraUid,
    required this.callSessionId,
    required this.isVideo,
    required this.callApiService,
  });

  @override
  State<CallScreen> createState() => _CallScreenState();
}

class _CallScreenState extends State<CallScreen> {
  // AgoraClient handles everything: joining, leaving, video rendering
  late final AgoraClient _client;

  @override
  void initState() {
    super.initState();

    _client = AgoraClient(
      agoraConnectionData: AgoraConnectionData(
        // ─── IMPORTANT ───
        // Get your App ID from https://console.agora.io
        // This is public and safe to include in the app (token is the secret part)
        appId: 'YOUR_AGORA_APP_ID',

        // These 3 values come from your backend response
        channelName: widget.channelName,
        tempToken: widget.token,
        uid: widget.agoraUid,
      ),
      enabledPermission: [Permission.camera, Permission.microphone],
    );

    _initAgora();
  }

  Future<void> _initAgora() async {
    await _client.initialize();

    // Update call status to "Ongoing" once connected
    widget.callApiService.updateCallStatus(widget.callSessionId, 1);
  }

  @override
  void dispose() {
    _client.release();
    super.dispose();
  }

  /// Called when the user taps the end-call button.
  Future<void> _onCallEnd() async {
    // Update call status to "Completed" on the backend
    await widget.callApiService.updateCallStatus(widget.callSessionId, 2);

    if (mounted) {
      Navigator.of(context).pop();
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: SafeArea(
        child: Stack(
          children: [
            // This single widget renders the entire call UI:
            // - Local video preview
            // - Remote user video
            // - Mute, camera toggle, end call buttons
            AgoraVideoViewer(
              client: _client,
              layoutType: Layout.floating, // or Layout.grid
              showNumberOfUsers: true,
            ),

            // Bottom toolbar with mute, camera, end-call buttons
            AgoraVideoButtons(
              client: _client,
              onDisconnect: _onCallEnd,
            ),
          ],
        ),
      ),
    );
  }
}
```

### Step 4: Initiate a Call (from your Chat Screen)

```dart
// Inside your chat screen or wherever the call button lives

Future<void> _startCall({required bool isVideo}) async {
  // 1. Request permissions
  final granted = await requestCallPermissions(isVideo: isVideo);
  if (!granted) {
    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(content: Text('Camera/Microphone permission denied')),
    );
    return;
  }

  // 2. Get token from your backend
  final callType = isVideo ? 0 : 1;
  final tokenResponse = await callApiService.generateCallToken(chatId, callType);

  // 3. Navigate to the call screen
  Navigator.of(context).push(
    MaterialPageRoute(
      builder: (_) => CallScreen(
        token: tokenResponse.token,
        channelName: tokenResponse.channelName,
        agoraUid: tokenResponse.agoraUid,
        callSessionId: tokenResponse.callSessionId,
        isVideo: isVideo,
        callApiService: callApiService,
      ),
    ),
  );
}
```

Add call buttons in your chat app bar:

```dart
AppBar(
  actions: [
    // Voice call button
    IconButton(
      icon: const Icon(Icons.call),
      onPressed: () => _startCall(isVideo: false),
    ),
    // Video call button
    IconButton(
      icon: const Icon(Icons.videocam),
      onPressed: () => _startCall(isVideo: true),
    ),
  ],
)
```

---

## 6. Complete Call Flow

### Caller Side (who initiates)

```
1. User taps call button (video or voice)
2. App requests camera/mic permissions
3. App calls POST /api/chats/{chatId}/call  →  gets token + channelName
4. App opens CallScreen with the token
5. AgoraClient.initialize() joins the Agora channel
6. App calls PUT /api/chats/calls/{id}/status  with status=1 (Ongoing)
7. User sees their own video, waits for the other person
8. When the other person joins, they see each other
9. User taps end call
10. App calls PUT /api/chats/calls/{id}/status  with status=2 (Completed)
11. Navigator.pop() back to chat
```

### Receiver Side (who gets the call)

```
1. Receiver gets a push notification or SignalR message:
   "Incoming call from X" with callSessionId and channelName
2. Receiver chooses to Accept or Reject

   If ACCEPT:
   3. App calls POST /api/chats/{chatId}/call  →  gets token for the SAME channel
      (backend returns the existing active session — reconnection/idempotency)
   4. App opens CallScreen, joins the same channel
   5. Both users are now connected

   If REJECT:
   3. App calls PUT /api/chats/calls/{id}/status  with status=4 (Rejected)
```

> [!NOTE]
> The receiver also calls `POST /api/chats/{chatId}/call` to get their own token. The backend detects the existing active session and returns a token for the **same channel** — it does NOT create a new session. This is the reconnection/idempotency logic.

### Missed Call

```
If the receiver doesn't respond within your timeout (e.g., 30 seconds):
1. Caller's app calls PUT /api/chats/calls/{id}/status  with status=3 (Missed)
2. Caller's app closes the CallScreen
```

---

## 7. Error Handling

```dart
Future<void> _startCall({required bool isVideo}) async {
  try {
    final granted = await requestCallPermissions(isVideo: isVideo);
    if (!granted) {
      _showError('Please allow camera and microphone access');
      return;
    }

    final callType = isVideo ? 0 : 1;
    final tokenResponse = await callApiService.generateCallToken(chatId, callType);

    Navigator.of(context).push(
      MaterialPageRoute(
        builder: (_) => CallScreen(/* ... */),
      ),
    );
  } on DioException catch (e) {
    final statusCode = e.response?.statusCode;
    final message = e.response?.data?['error']?['message'] ?? 'Something went wrong';

    if (statusCode == 401) {
      _showError('You are not authorized to make this call');
    } else {
      _showError(message);
    }
  }
}
```

> [!TIP]
> **Agora App ID**: You need to get this from [https://console.agora.io](https://console.agora.io). Create a project, copy the App ID, and use it in `AgoraConnectionData.appId`. The App ID is public — the security comes from the **token** generated by your backend.

---

## Quick Checklist

- [ ] Add `agora_rtc_engine`, `agora_uikit`, `dio`, `permission_handler` to pubspec
- [ ] Set Android `minSdkVersion 24`
- [ ] Add camera/mic permissions to AndroidManifest.xml
- [ ] Add camera/mic usage descriptions to iOS Info.plist
- [ ] Get your Agora App ID from console.agora.io
- [ ] Create `CallApiService` to call your backend
- [ ] Create `CallScreen` using `AgoraVideoViewer` + `AgoraVideoButtons`
- [ ] Add call buttons to your chat screen
- [ ] Handle permissions, errors, and call status updates
