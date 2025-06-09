using CodePromptus.App.Configuration;
using CodePromptus.App.Infrastructure;
using CodePromptus.App.Models;
using Fluid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodePromptus.App.Services;

public class PromptGeneratorService
{
    private readonly IFileSystemService _fileSystemService;
    private readonly string _templatePath;
    private readonly FluidParser _parser;
    private readonly TemplateOptions _templateOptions;

    public PromptGeneratorService(IFileSystemService fileSystemService)
    {
        _fileSystemService = fileSystemService;
        _templatePath = Constants.TemplatePath;
        _parser = new FluidParser();

        _templateOptions = new TemplateOptions();
        _templateOptions.MemberAccessStrategy.Register<FileSystemItem>();
        _templateOptions.MemberAccessStrategy.Register(new { Path = "", Content = "" }.GetType());
        _templateOptions.MemberAccessStrategy.Register<List<dynamic>>();
        _templateOptions.MemberAccessStrategy.Register<IEnumerable<dynamic>>();
        _templateOptions.MemberAccessStrategy.Register<string>();
        _templateOptions.MemberAccessStrategy.Register<bool>();
    }

    public async Task<string> GeneratePromptAsync(IEnumerable<FileSystemItem> rootItems)
    {
        if (!_fileSystemService.FileExists(_templatePath))
        {
            throw new System.IO.FileNotFoundException($"Template file not found: {_templatePath}");
        }
        var templateContent = await _fileSystemService.ReadFileContentAsync(_templatePath);
        if (!_parser.TryParse(templateContent, out var template, out var error))
        {
            throw new Exception($"Failed to parse template: {error}");
        }

        var hierarchyBuilder = new StringBuilder();
        foreach (var rootItem in rootItems)
        {
            await AppendHierarchyAsync(hierarchyBuilder, rootItem, 0);
        }

        var selectedFilesContent = new List<dynamic>();

        var selectedFiles = rootItems.SelectMany(item => item.EnumerateDescendants())
            .Where(item => item.IsSelected && !item.IsDirectory).OrderBy(i => i.FullPath);
        foreach(var file in selectedFiles)
        {
            string content = await _fileSystemService.ReadFileContentAsync(file.FullPath);
            selectedFilesContent.Add(new
            {
                Path = file.RelativePath,
                Content = content
            });
        }

        var context = new TemplateContext(_templateOptions);
        context.SetValue("hierarchy", hierarchyBuilder.ToString());
        context.SetValue("selected_files", selectedFilesContent);
        context.SetValue("has_selected_files", selectedFilesContent.Count != 0);
        
        try
        {
            return await template.RenderAsync(context);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to render template: {ex.Message}\n{ex.StackTrace}", ex);
        }
    }

    private static async Task AppendHierarchyAsync(StringBuilder sb, FileSystemItem item, int level)
    {
        string indent = new(' ', level * 2);
        sb.AppendLine($"{indent}{item.Name}{(item.IsDirectory ? "/" : "")}");

        if (item.IsDirectory && item.Children.Count != 0)
        {
            foreach (var child in item.Children.OrderByDescending(c => c.IsDirectory).ThenBy(c => c.Name))
            {
                await AppendHierarchyAsync(sb, child, level + 1);
            }
        }
    }
}