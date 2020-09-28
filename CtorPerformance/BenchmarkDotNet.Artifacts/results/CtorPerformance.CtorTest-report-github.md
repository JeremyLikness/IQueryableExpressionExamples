``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19042
Intel Core i7-9850H CPU 2.60GHz, 1 CPU, 12 logical and 6 physical cores
.NET Core SDK=5.0.100-preview.5.20279.10
  [Host]     : .NET Core 5.0.0 (CoreCLR 5.0.20.27801, CoreFX 5.0.20.27801), X64 RyuJIT
  DefaultJob : .NET Core 5.0.0 (CoreCLR 5.0.20.27801, CoreFX 5.0.20.27801), X64 RyuJIT


```
|                   Method |      Mean |     Error |    StdDev | Ratio | RatioSD |
|------------------------- |----------:|----------:|----------:|------:|--------:|
|                    new() |  12.93 ns |  0.257 ns |  0.352 ns |  1.00 |    0.00 |
| Activator.CreateInstance | 504.97 ns | 10.053 ns | 10.323 ns | 39.06 |    1.40 |
|           Expression.New |  14.29 ns |  0.269 ns |  0.276 ns |  1.11 |    0.04 |
