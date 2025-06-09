using CodePromptus.App.Helpers;
using CodePromptus.App.Infrastructure;
using CodePromptus.App.Models;
using CodePromptus.App.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace CodePromptus.App.ViewModels;

public class PromptDisplayViewModel : ViewModelBase
{
    private const string CopyButtonTextDefault = "Copy to Clipboard";
    private readonly PromptGeneratorService _promptGeneratorService;
    private readonly ILogService<PromptDisplayViewModel> _logger;
    private readonly ToastService _toastService;

    [Reactive]
    public string GeneratedPrompt { get; set; } = string.Empty;
    [Reactive]
    public bool IsGeneratingPrompt { get; set; }
    [ObservableAsProperty]
    public int GeneratedPromptLength { get; }
    [Reactive]
    public string CopyButtonText { get; private set; } = CopyButtonTextDefault;

    public ReactiveCommand<Unit, Unit> CopyToClipboardCommand { get; }
#pragma warning disable CS8618
    public PromptDisplayViewModel() { } // For design-time data
#pragma warning restore CS8618

    public PromptDisplayViewModel(PromptGeneratorService promptGeneratorService,
                                 ILogService<PromptDisplayViewModel> logger,
                                 ToastService toastService) 
    {
        _promptGeneratorService = promptGeneratorService;
        _logger = logger;
        _toastService = toastService;

        this.WhenAnyValue(x => x.GeneratedPrompt)
            .Select(prompt => prompt.Length)
            .ToPropertyEx(this, x => x.GeneratedPromptLength);

        CopyToClipboardCommand = ReactiveCommand.CreateFromTask(CopyPromptToClipboardAsync);
    }

    public async Task GeneratePromptAsync(IEnumerable<FileSystemItem> RootItems)
    {
        try
        {
            IsGeneratingPrompt = true;
            GeneratedPrompt = await _promptGeneratorService.GeneratePromptAsync(RootItems);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error has occurred while generating prompt");
            _toastService.ShowError("Failed to generate prompt. Please try again.");
        }
        finally
        {
            IsGeneratingPrompt = false;
        }
    }

    private async Task CopyPromptToClipboardAsync()
    {
        if (!string.IsNullOrEmpty(GeneratedPrompt))
        {
            await ClipboardHelper.CopyToClipboardAsync(GeneratedPrompt);
            CopyButtonText = "Copied!";
            await Task.Delay(1500);
            CopyButtonText = CopyButtonTextDefault;
        }
    }
}