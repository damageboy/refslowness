using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.CsProj;
using xxx;

namespace refslowness
{
    public class Config : ManualConfig
    {
        public Config()
        {
            Add(Job.ShortRun.With(Jit.RyuJit).With(Platform.X64).With(Runtime.Clr));
            Add(Job.ShortRun.With(Jit.RyuJit).With(Platform.X64).With(Runtime.Core).With(CsProjCoreToolchain.NetCoreApp20));

            
            Add(StatisticColumn.Min);
            Add(StatisticColumn.Max);
            Add(DisassemblyDiagnoser.Create(new DisassemblyDiagnoserConfig(printAsm: true, printPrologAndEpilog: true, recursiveDepth: 3)));
        }
    }
    
    public class ConfigSlow : ManualConfig
    {
        public ConfigSlow()
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
            });

            competition.Run(args);
        }
    }

    
    [Config(typeof(ConfigSlow))]
    public class CircularBufferBenchmarks
    {
        CircularBuffer<double> _cb;
        CircularBufferRef<double> _crb;
        
        [GlobalSetup]
        public void Setup()
        {
            _cb = new CircularBuffer<double>(1024);
            _crb = new CircularBufferRef<double>(1024);

            foreach (var d in Enumerable.Range(0, 1024))
            {
                _cb.Enqueue(d);
                _crb.Enqueue(d);
            }
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
        
        [Benchmark(OperationsPerInvoke = 10)]
        public double IndexerRef()
        {
            double x = 0 ;
            var c = _crb.Count - 1;
            for (var i = 0; i < 10; i++)
                x += _crb[c];
            return x;
        }
        
    }
}

