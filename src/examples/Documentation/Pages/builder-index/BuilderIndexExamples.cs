// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Binding.Mixins;
using ReactiveUI.Binding.ObservableForProperty;
using ReactiveUI.Primitives;
using Splat;
using Splat.Builder;

namespace ReactiveUI.Binding.Documentation.BuilderIndex;

/// <summary>Demonstrates how an application configures ReactiveUI.Binding and builds its instance.</summary>
public static class BuilderIndexExamples
{
    /// <summary>The number of observable-for-property services the core module registers.</summary>
    private const int CoreObservationServiceCount = 2;

    /// <summary>The title of the first seeded item.</summary>
    private const string OriginalTitle = "Renew car registration";

    /// <summary>The title given to an item while it is observed.</summary>
    private const string RenamedTitle = "Renew car registration online";

    /// <summary>The tags of the first seeded item, as the tag converter writes them.</summary>
    private const string SeededTagsText = "car, admin";

    /// <summary>The name of the highest to-do priority.</summary>
    private const string HighPriorityName = "High";

    /// <summary>Creates a builder over the application-wide Splat locator.</summary>
    public static void CreateBuilder()
    {
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();

        SampleCheck.Equal(true, builder.ConverterService.TypedConverters.TryGetConverter(typeof(int), typeof(string)) is null);
    }

    /// <summary>Creates a builder over a resolver you own, through the resolver extension or the constructor.</summary>
    public static void CreateBuilderForResolver()
    {
        using ModernDependencyResolver resolver = new();
        var fromResolver = resolver.CreateReactiveUIBindingBuilder();

        SampleCheck.Equal(true, ReferenceEquals(resolver, fromResolver.CurrentMutable));

        using ModernDependencyResolver other = new();
        ReactiveUIBindingBuilder direct = new(other, null);

        SampleCheck.Equal(true, ReferenceEquals(other, direct.CurrentMutable));
    }

    /// <summary>Shows that binding refuses to run before <c>BuildApp</c>.</summary>
    public static void EnsureInitializedNeedsBuildApp()
    {
        var threw = false;

        try
        {
            RxBindingBuilder.EnsureInitialized();
        }
        catch (InvalidOperationException)
        {
            threw = true;
        }

        SampleCheck.Equal(true, threw);
    }

    /// <summary>Builds the to-do application: core services, a module, converters, a command binder, a provider and view mappings.</summary>
    /// <param name="provider">The provider that observes <see cref="TodoItem"/> properties.</param>
    /// <param name="binder">The binder that attaches commands to buttons.</param>
    /// <returns>The built application instance.</returns>
    public static IReactiveUIBindingInstance BuildTodoApplication(TodoItemObservableForProperty provider, ButtonCommandBinder binder)
    {
        IReactiveUIBindingBuilder builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();

        return builder
            .WithCoreServices()
            .WithPlatformModule(new TodoModule())
            .WithConverter(new TagListToStringConverter())
            .WithFallbackConverter(new EnumNameFallbackConverter())
            .WithSetMethodConverter(new TagListSetMethodConverter())
            .WithCommandBinder(binder)
            .WithRegistration(resolver => resolver.RegisterLazySingleton<ICreatesObservableForProperty>(() => provider))
            .ConfigureViewLocator(static mappings => mappings.Map<TodoListViewModel, TodoView>())
            .BuildApp();
    }

    /// <summary>Reads the service that the module registered into the built application.</summary>
    /// <param name="app">The application returned by <see cref="BuildTodoApplication"/>.</param>
    /// <returns>The to-do database the module registered.</returns>
    /// <exception cref="InvalidOperationException">The module did not register the to-do database.</exception>
    public static ITodoStore ResolveModuleService(IReactiveUIBindingInstance app)
    {
        if (app.Current?.GetService<ITodoStore>() is not InMemoryTodoStore store)
        {
            throw new InvalidOperationException("The module did not register the to-do database.");
        }

        return store;
    }

    /// <summary>Shows that <c>BuildApp</c> marks binding as initialized.</summary>
    /// <param name="app">The application returned by <see cref="BuildTodoApplication"/>.</param>
    public static void EnsureInitializedAfterBuildApp(IReactiveUIBindingInstance app)
    {
        RxBindingBuilder.EnsureInitialized();

        SampleCheck.Equal(true, app.Current is not null);
    }

    /// <summary>Observes an item through the provider the builder registered; the provider outranks the generated observation.</summary>
    /// <param name="item">The item to observe.</param>
    /// <param name="provider">The provider registered by <see cref="BuildTodoApplication"/>.</param>
    public static void ObserveThroughRegisteredProvider(TodoItem item, TodoItemObservableForProperty provider)
    {
        List<string> titles = [];

        using (item.WhenChanged(x => x.Title).Subscribe(titles.Add))
        {
            item.Title = RenamedTitle;
        }

        SampleCheck.SequenceEqual([OriginalTitle, RenamedTitle], titles);
        SampleCheck.Equal(1, provider.ObservationCount);
    }

    /// <summary>Binds tags to a label; the typed converter the builder registered turns the list into text.</summary>
    /// <param name="viewModel">The view model that holds the selected item.</param>
    /// <param name="view">The view that shows the tags.</param>
    public static void ConvertTagsWithRegisteredConverter(TodoListViewModel viewModel, TodoView view)
    {
        viewModel.SelectedItem = viewModel.Items[0];

        using var binding = view.OneWayBind(viewModel, x => x.SelectedItem!.Tags, v => v.TagsLabel.Text);

        SampleCheck.Equal(SeededTagsText, view.TagsLabel.Text);
    }

    /// <summary>Converts an enumeration with the fallback converter the builder registered.</summary>
    public static void ConvertPriorityWithRegisteredFallbackConverter()
    {
        var converter = BindingConverters.Current.FallbackConverters.TryGetConverter(typeof(TodoPriority), typeof(string));

        SampleCheck.Equal(true, converter is EnumNameFallbackConverter);

        _ = converter!.TryConvert(typeof(TodoPriority), TodoPriority.High, typeof(string), null, out var name);

        SampleCheck.Equal(HighPriorityName, name);
    }

    /// <summary>Writes a tag by index with the set-method converter the builder registered.</summary>
    public static void SetIndexedTagWithRegisteredSetMethodConverter()
    {
        var converter = BindingConverters.Current.ResolveSetMethodConverter(typeof(List<string>), typeof(string));

        SampleCheck.Equal(true, converter is TagListSetMethodConverter);

        List<string> tags = ["car", "admin"];
        _ = converter!.PerformSet(tags, "money", [1]);

        SampleCheck.SequenceEqual(["car", "money"], tags);
    }

    /// <summary>Binds a command to a button; the command binder the builder registered attaches it.</summary>
    /// <param name="viewModel">The view model that owns the command.</param>
    /// <param name="view">The view that owns the button.</param>
    /// <param name="binder">The binder registered by <see cref="BuildTodoApplication"/>.</param>
    public static void AttachCommandWithRegisteredBinder(TodoListViewModel viewModel, TodoView view, ButtonCommandBinder binder)
    {
        using var binding = view.BindCommand(viewModel, x => x.AddCommand, v => v.AddButton);

        SampleCheck.Equal(1, binder.BindCount);
        SampleCheck.Equal(true, ReferenceEquals(viewModel.AddCommand, view.AddButton.Command));
    }

    /// <summary>Finds the view for a view model through the mapping the builder registered.</summary>
    /// <param name="viewModel">The view model to find a view for.</param>
    public static void ResolveMappedView(TodoListViewModel viewModel)
    {
        var view = ViewLocator.GetCurrent().ResolveView(viewModel);

        SampleCheck.Equal(true, view is TodoView);
    }

    /// <summary>Registers the core observation providers with the module, without a builder.</summary>
    public static void RegisterCoreObservationModule()
    {
        using ModernDependencyResolver resolver = new();
        ReactiveUIBindingModule module = new();

        module.Configure(resolver);

        List<Type> providers = [];
        foreach (var service in resolver.GetServices<ICreatesObservableForProperty>())
        {
            providers.Add(service.GetType());
        }

        SampleCheck.Equal(CoreObservationServiceCount, providers.Count);
        SampleCheck.Equal(true, providers.Contains(typeof(INPCObservableForProperty)));
        SampleCheck.Equal(true, providers.Contains(typeof(POCOObservableForProperty)));
    }

    /// <summary>Registers the three kinds of converter through the <see cref="IAppBuilder"/> extensions.</summary>
    public static void RegisterConvertersThroughAppBuilder()
    {
        using ModernDependencyResolver resolver = new();
        var builder = resolver.CreateReactiveUIBindingBuilder();
        IAppBuilder appBuilder = builder;

        _ = appBuilder
            .WithConverter(new TagListToStringConverter())
            .WithFallbackConverter(new EnumNameFallbackConverter())
            .WithSetMethodConverter(new TagListSetMethodConverter());

        SampleCheck.Equal(true, builder.ConverterService.TypedConverters.TryGetConverter(typeof(IReadOnlyList<string>), typeof(string)) is TagListToStringConverter);
        SampleCheck.Equal(true, builder.ConverterService.FallbackConverters.TryGetConverter(typeof(TodoPriority), typeof(string)) is EnumNameFallbackConverter);
        SampleCheck.Equal(true, builder.ConverterService.SetMethodConverters.TryGetConverter(typeof(List<string>), typeof(string)) is TagListSetMethodConverter);
    }

    /// <summary>Registers a service and view mappings through the <see cref="IAppBuilder"/> extensions.</summary>
    public static void ConfigureThroughAppBuilder()
    {
        using ModernDependencyResolver resolver = new();
        var builder = resolver.CreateReactiveUIBindingBuilder();
        IAppBuilder appBuilder = builder;

        SampleCheck.Equal(true, ReferenceEquals(builder, appBuilder.WithPlatformModule(new TodoModule())));

        _ = appBuilder
            .WithRegistration(static registry => registry.RegisterConstant(InMemoryTodoStore.CreateSeeded()))
            .ConfigureViewLocator(static mappings => mappings.Map<TodoListViewModel, TodoView>());

        var locator = resolver.GetService<IViewLocator>();
        TodoListViewModel viewModel = new(resolver.GetService<InMemoryTodoStore>()!);

        SampleCheck.Equal(true, locator!.ResolveView(viewModel) is TodoView);
    }

    /// <summary>Builds an application through the <see cref="IAppBuilder"/> extension.</summary>
    public static void BuildThroughAppBuilder()
    {
        using ModernDependencyResolver resolver = new();
        IAppBuilder appBuilder = resolver.CreateReactiveUIBindingBuilder();

        var app = appBuilder.BuildApp();

        SampleCheck.Equal(true, app.Current is not null);
    }
}
