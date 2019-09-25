# ReflectionToIL

A demonstration and benchmark of different approaches to load closure fields. This is a companion project for my Medium post [**"Optimizing reflection in C# via dynamic code generation"**](https://medium.com/@SergioPedri/optimizing-reflection-with-dynamic-code-generation-6e15cef4b1a2).

<a href="https://www.nuget.org/packages/UICompositionAnimations/"><img src="https://user-images.githubusercontent.com/10199417/65376126-a30c1e00-dc9c-11e9-9754-24b24ef3e1d2.png" alt="Medium blog post" width='960' /></a>

### Project contents

The project is ready to run, and it uses `BenchmarkDotNet` to execute the benchmarks: you can find more info about this by opening the `Program.cs` file. The project also contains 4 different implementations of the closure loader discussed in the blog post, which are located in the `\Implementations` folder.
