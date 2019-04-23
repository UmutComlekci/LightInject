namespace LightInject
{
#if NET452 || NETSTANDARD1_3 || NETSTANDARD1_6 || NETSTANDARD2_0 || NET46 || NETCOREAPP2_0

    /// <summary>
    /// Manages a set of <see cref="Scope"/> instances.
    /// </summary>
    public class PerLogicalCallContextScopeManager : ScopeManager
    {
        private readonly LogicalThreadStorage<Scope> currentScope = new LogicalThreadStorage<Scope>();

        /// <summary>
        /// Initializes a new instance of the <see cref="PerLogicalCallContextScopeManager"/> class.
        /// </summary>
        /// <param name="serviceFactory">The <see cref="IServiceFactory"/> to be associated with this <see cref="ScopeManager"/>.</param>
        public PerLogicalCallContextScopeManager(IServiceFactory serviceFactory)
            : base(serviceFactory)
        {
        }

        /// <summary>
        /// Gets or sets the current <see cref="Scope"/>.
        /// </summary>
        public override Scope CurrentScope
        {
            get { return GetThisScopeOrFirstValidAncestor(currentScope.Value); }
            set { currentScope.Value = value; }
        }
    }

    /// <summary>
    /// A <see cref="IScopeManagerProvider"/> that creates an <see cref="IScopeManager"/>
    /// that is capable of managing scopes across async points.
    /// </summary>
    public class PerLogicalCallContextScopeManagerProvider : ScopeManagerProvider
    {
        /// <summary>
        /// Creates a new <see cref="IScopeManager"/> instance.
        /// </summary>
        /// <param name="serviceFactory">The <see cref="IServiceFactory"/> to be associated with the <see cref="IScopeManager"/>.</param>
        /// <returns><see cref="IScopeManager"/>.</returns>
        protected override IScopeManager CreateScopeManager(IServiceFactory serviceFactory)
        {
            return new PerLogicalCallContextScopeManager(serviceFactory);
        }
    }
#endif
}