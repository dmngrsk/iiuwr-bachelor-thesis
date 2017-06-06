``` ini

BenchmarkDotNet=v0.10.7, OS=linuxmint 18.1
Processor=Intel Core i7-5500U CPU 2.40GHz (Broadwell), ProcessorCount=4
Frequency=1000000000 Hz, Resolution=1.0000 ns, Timer=UNKNOWN
dotnet cli version=1.0.0-rc4-004771
  [Host]     : .NET Core 4.6.24628.01, 64bit RyuJITDEBUG
  DefaultJob : .NET Core 4.6.24628.01, 64bit RyuJIT


```
 |                                    Method |       Mean |     Error |    StdDev |
 |------------------------------------------ |-----------:|----------:|----------:|
 |                                select_all |   4.464 ms | 0.0850 ms | 0.1012 ms |
 | select_columns_creating_an_anonymous_type |   4.285 ms | 0.0969 ms | 0.0907 ms |
 |                         select_with_where |   4.043 ms | 0.0789 ms | 0.0877 ms |
 |        select_with_multiconditional_where |   4.362 ms | 0.0850 ms | 0.0909 ms |
 |               select_with_multiple_wheres |   4.392 ms | 0.0643 ms | 0.0601 ms |
 |                   handles_string_addition |   3.813 ms | 0.0515 ms | 0.0430 ms |
 |                       logical_not_applied |   4.361 ms | 0.0869 ms | 0.0966 ms |
 |     select_with_multiple_orderings_joined |   4.498 ms | 0.0899 ms | 0.1346 ms |
 |      select_with_multiple_orderings_split |   4.492 ms | 0.0886 ms | 0.1020 ms |
 | select_with_additional_from_as_cross_join |  10.759 ms | 0.0562 ms | 0.0498 ms |
 |                    select_with_inner_join |  10.147 ms | 0.0679 ms | 0.0602 ms |
 |                    select_with_group_join | 102.615 ms | 0.6347 ms | 0.5626 ms |
 |                    select_with_outer_join |  10.244 ms | 0.0628 ms | 0.0557 ms |
 |                          select_with_case |   4.128 ms | 0.0792 ms | 0.0741 ms |
