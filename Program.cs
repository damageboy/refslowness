using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

namespace xxx
{
    public class Config : ManualConfig
    {
        public Config()
        {
            Add(new Job()
            {
                Env = { Runtime = Runtime.Clr, Jit = Jit.RyuJit , Platform = Platform.X64 },
                Run = { LaunchCount = 1, WarmupCount = 1, TargetCount = 10 },
            });
            //Add(Job.Mono);
            Add(StatisticColumn.Min);
            Add(StatisticColumn.Max);
            //Add(new MemoryDiagnoser());
            Add(RPlotExporter.Default);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var competition = new BenchmarkSwitcher(new[] {
                typeof(CircularBufferBenchmarks),
                typeof(CircularBufferRefBenchmarks),
            });

            competition.Run(args);
        }
    }

    
    [Config(typeof(Config))]
    public class CircularBufferBenchmarks
    {
        CircularBuffer<double> _cb;
        [GlobalSetup]
        public void Setup()
        {
            _cb = new CircularBuffer<double>(1024);
            foreach (var d in Enumerable.Range(0, 1024))
                _cb.Enqueue(d);
        }

        [Benchmark(OperationsPerInvoke = 10)]
        public double Indexer()
        {
            double x = 0 ;
            var c = _cb.Count - 1;
            for (var i = 0; i < 10; i++)
              x += _cb[c];
            return x;
        }
    }
    
    [Config(typeof(Config))]
    public class CircularBufferRefBenchmarks
    {
        CircularBufferRef<double> _cb;
        [GlobalSetup]
        public void Setup()
        {
            _cb = new CircularBufferRef<double>(1024);
            foreach (var d in Enumerable.Range(0, 1024))
                _cb.Enqueue(d);
        }

        [Benchmark(OperationsPerInvoke = 10)]
        public double Indexer()
        {
            double x = 0 ;
            var c = _cb.Count - 1;
            for (var i = 0; i < 10; i++)
                x += _cb[c];
            return x;
        }
    }
}

