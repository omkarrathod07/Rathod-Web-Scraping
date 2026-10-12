using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace RathodWebScraping.Services;

public class VoiceSpeechService : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private DotNetObjectReference<VoiceSpeechService>? _dotNetHelper;
    private Action<string>? _onTranscriptReceivedCallback;
    private Action<string>? _onErrorCallback;

    public VoiceSpeechService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }
    public async Task StartSpeechToTextAsync(Action<string> onTranscriptReceived, Action<string> onError)
    {
        _onTranscriptReceivedCallback = onTranscriptReceived;
        _onErrorCallback = onError;
        _dotNetHelper ??= DotNetObjectReference.Create(this);

        await _jsRuntime.InvokeVoidAsync("blazorVoice.startSpeechRecognition", _dotNetHelper, nameof(OnVoiceCapturedInternal), nameof(OnVoiceErrorInternal));
    }
    public async Task StopSpeechToTextAsync()
    {
        await _jsRuntime.InvokeVoidAsync("blazorVoice.stopSpeechRecognition");
    }
    public async Task PlayTextToSpeechAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        await _jsRuntime.InvokeVoidAsync("blazorVoice.playTextToSpeech", text);
    }
    [JSInvokable]
    public void OnVoiceCapturedInternal(string transcript)
    {
        _onTranscriptReceivedCallback?.Invoke(transcript);
    }
    [JSInvokable]
    public void OnVoiceErrorInternal(string errorMessage)
    {
        _onErrorCallback?.Invoke(errorMessage);
    }

    public async ValueTask DisposeAsync()
    {
        if (_dotNetHelper != null)
        {
            _dotNetHelper.Dispose();
            _dotNetHelper = null;
        }
        GC.SuppressFinalize(this);
    }
}