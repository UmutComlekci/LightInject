namespace LightInject
{
   /// <summary>
    /// A <see cref="IScopeManager"/> that manages scopes per thread.
    /// </summary>
    public class PerThreadScopeManager : ScopeManager
    {
        private readonly ThreadLocal<Scope> threadLocalScope = new ThreadLocal<Scope>();

        /// <summary>
        /// Initializes a new instance of the <see cref="PerThreadScopeManager"/> class.
        /// </summary>
        /// <param name="serviceFactory">The <see cref="IServiceFactory"/> to be associated with this <see cref="ScopeManager"/>.</param>
        public PerThreadScopeManager(IServiceFactory serviceFactory)
            : base(serviceFactory)
        {
        }

        /// <summary>
        /// Gets or sets the current <see cref="Scope"/>.
        /// </summary>
        public override Scope CurrentScope
        {
            get { return GetThisScopeOrFirstValidAncestor(threadLocalScope.Value); }
            set { threadLocalScope.Value = value; }
        }
    }
}