using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AppDimens.Maui.Markup;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace AppDimens.Maui.BuildTasks;

public sealed class ExpandShortDimenMarkup : Microsoft.Build.Utilities.Task
{
    [Required]
    public ITaskItem[] Inputs { get; set; } = Array.Empty<ITaskItem>();

    [Required]
    public string IntermediateOutputPath { get; set; } = string.Empty;

    [Output]
    public ITaskItem[] Outputs { get; set; } = Array.Empty<ITaskItem>();

    public override bool Execute()
    {
        var outputs = new List<ITaskItem>();
        var outRoot = Path.Combine(IntermediateOutputPath, "AppDimensExpandedXaml");
        Directory.CreateDirectory(outRoot);

        foreach (var input in Inputs)
        {
            var sourcePath = input.ItemSpec;
            if (!File.Exists(sourcePath))
            {
                Log.LogError($"XAML not found: {sourcePath}");
                return false;
            }

            var relative = input.GetMetadata("RelativePath");
            if (string.IsNullOrEmpty(relative))
                relative = input.GetMetadata("TargetPath");
            if (string.IsNullOrEmpty(relative))
                relative = Path.GetFileName(sourcePath);

            var content = File.ReadAllText(sourcePath);
            var expanded = ShortDimenMarkupExpander.Expand(content);

            TaskItem output;
            if (string.Equals(content, expanded, StringComparison.Ordinal))
            {
                output = new TaskItem(sourcePath);
            }
            else
            {
                var destPath = Path.Combine(outRoot, relative.Replace('\\', Path.DirectorySeparatorChar));
                var destDir = Path.GetDirectoryName(destPath);
                if (!string.IsNullOrEmpty(destDir))
                    Directory.CreateDirectory(destDir);
                File.WriteAllText(destPath, expanded, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
                output = new TaskItem(destPath);
                output.SetMetadata("OriginalFullPath", sourcePath);
                Log.LogMessage(MessageImportance.Low, $"Expanded AppDimens short markup: {relative}");
            }

            output.SetMetadata("RelativePath", relative);
            var inflator = input.GetMetadata("Inflator");
            if (!string.IsNullOrEmpty(inflator))
                output.SetMetadata("Inflator", inflator);

            outputs.Add(output);
        }

        Outputs = outputs.ToArray();
        return !Log.HasLoggedErrors;
    }
}
