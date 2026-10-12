using Xunit;

// Several tests swap the process-wide AppDimensAmbient context: run them sequentially.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
