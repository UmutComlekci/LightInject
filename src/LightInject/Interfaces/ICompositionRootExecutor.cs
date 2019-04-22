namespace LightInject
{
   /// <summary>
    /// Represents a class that is responsible for instantiating and executing an <see cref="ICompositionRoot"/>.
    /// </summary>
    public interface ICompositionRootExecutor
    {
        /// <summary>
        /// Creates an instance of the <paramref name="compositionRootType"/> and executes the <see cref="ICompositionRoot.Compose"/> method.
        /// </summary>
        /// <param name="compositionRootType">The concrete <see cref="ICompositionRoot"/> type to be instantiated and executed.</param>
        void Execute(Type compositionRootType);
    }
}