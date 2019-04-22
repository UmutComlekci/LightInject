namespace LightInject
{
    /// <summary>
    /// A class that is responsible for instantiating and executing an <see cref="ICompositionRoot"/>.
    /// </summary>
    public class CompositionRootExecutor : ICompositionRootExecutor
    {
        private readonly IServiceRegistry serviceRegistry;
        private readonly Func<Type, ICompositionRoot> activator;

        private readonly IList<Type> executedCompositionRoots = new List<Type>();

        private readonly object syncRoot = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositionRootExecutor"/> class.
        /// </summary>
        /// <param name="serviceRegistry">The <see cref="IServiceRegistry"/> to be configured by the <see cref="ICompositionRoot"/>.</param>
        /// <param name="activator">The function delegate that is responsible for creating an instance of the <see cref="ICompositionRoot"/>.</param>
        public CompositionRootExecutor(IServiceRegistry serviceRegistry, Func<Type, ICompositionRoot> activator)
        {
            this.serviceRegistry = serviceRegistry;
            this.activator = activator;
        }

        /// <summary>
        /// Creates an instance of the <paramref name="compositionRootType"/> and executes the <see cref="ICompositionRoot.Compose"/> method.
        /// </summary>
        /// <param name="compositionRootType">The concrete <see cref="ICompositionRoot"/> type to be instantiated and executed.</param>
        public void Execute(Type compositionRootType)
        {
            if (!executedCompositionRoots.Contains(compositionRootType))
            {
                lock (syncRoot)
                {
                    if (!executedCompositionRoots.Contains(compositionRootType))
                    {
                        executedCompositionRoots.Add(compositionRootType);
                        var compositionRoot = activator(compositionRootType);
                        compositionRoot.Compose(serviceRegistry);
                    }
                }
            }
        }
    }
}