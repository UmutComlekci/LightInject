namespace LightInject
{
    /// <summary>
    /// A generator that transforms <see cref="OpCodes"/> into an expression tree.
    /// </summary>
    public class ILGenerator
    {
        private readonly ParameterExpression[] parameters;
        private readonly Stack<Expression> stack = new Stack<Expression>();
        private readonly List<LocalBuilder> locals = new List<LocalBuilder>();
        private readonly List<Expression> expressions = new List<Expression>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ILGenerator"/> class.
        /// </summary>
        /// <param name="parameters">An array of parameters used by the target <see cref="DynamicMethod"/>.</param>
        public ILGenerator(ParameterExpression[] parameters)
        {
            this.parameters = parameters;
        }

        /// <summary>
        /// Gets the current expression based the emitted <see cref="OpCodes"/>.
        /// </summary>
        public Expression CurrentExpression
        {
            get
            {
                var variables = locals.Select(l => l.Variable).ToList();
                var ex = new List<Expression>(expressions) { stack.Peek() };
                return Expression.Block(variables, ex);
            }
        }

        /// <summary>
        /// Puts the specified instruction and metadata token for the specified constructor onto the Microsoft intermediate language (MSIL) stream of instructions.
        /// </summary>
        /// <param name="code">The MSIL instruction to be emitted onto the stream.</param>
        /// <param name="constructor">A <see cref="ConstructorInfo"/> representing a constructor.</param>
        public void Emit(OpCode code, ConstructorInfo constructor)
        {
            if (code == OpCodes.Newobj)
            {
                var parameterCount = constructor.GetParameters().Length;
                var expression = Expression.New(constructor, Pop(parameterCount));
                stack.Push(expression);
            }
            else
            {
                throw new NotSupportedException(code.ToString());
            }
        }

        /// <summary>
        /// Puts the specified instruction onto the stream of instructions.
        /// </summary>
        /// <param name="code">The Microsoft Intermediate Language (MSIL) instruction to be put onto the stream.</param>
        public void Emit(OpCode code)
        {
            if (code == OpCodes.Ldarg_0)
            {
                stack.Push(parameters[0]);
            }
            else if (code == OpCodes.Ldarg_1)
            {
                stack.Push(parameters[1]);
            }
            else if (code == OpCodes.Ldarg_2)
            {
                stack.Push(parameters[2]);
            }
            else if (code == OpCodes.Ldarg_3)
            {
                stack.Push(parameters[3]);
            }
            else if (code == OpCodes.Ldloc_0)
            {
                stack.Push(locals[0].Variable);
            }
            else if (code == OpCodes.Ldloc_1)
            {
                stack.Push(locals[1].Variable);
            }
            else if (code == OpCodes.Ldloc_2)
            {
                stack.Push(locals[2].Variable);
            }
            else if (code == OpCodes.Ldloc_3)
            {
                stack.Push(locals[3].Variable);
            }
            else if (code == OpCodes.Stloc_0)
            {
                Expression valueExpression = stack.Pop();
                var assignExpression = Expression.Assign(locals[0].Variable, valueExpression);
                expressions.Add(assignExpression);
            }
            else if (code == OpCodes.Stloc_1)
            {
                Expression valueExpression = stack.Pop();
                var assignExpression = Expression.Assign(locals[1].Variable, valueExpression);
                expressions.Add(assignExpression);
            }
            else if (code == OpCodes.Stloc_2)
            {
                Expression valueExpression = stack.Pop();
                var assignExpression = Expression.Assign(locals[2].Variable, valueExpression);
                expressions.Add(assignExpression);
            }
            else if (code == OpCodes.Stloc_3)
            {
                Expression valueExpression = stack.Pop();
                var assignExpression = Expression.Assign(locals[3].Variable, valueExpression);
                expressions.Add(assignExpression);
            }
            else if (code == OpCodes.Ldelem_Ref)
            {
                Expression[] indexes = { stack.Pop() };
                for (int i = 0; i < indexes.Length; i++)
                {
                    indexes[0] = Expression.Convert(indexes[i], typeof(int));
                }

                Expression array = stack.Pop();
                stack.Push(Expression.ArrayAccess(array, indexes));
            }
            else if (code == OpCodes.Ldlen)
            {
                Expression array = stack.Pop();
                stack.Push(Expression.ArrayLength(array));
            }
            else if (code == OpCodes.Conv_I4)
            {
                stack.Push(Expression.Convert(stack.Pop(), typeof(int)));
            }
            else if (code == OpCodes.Ldc_I4_0)
            {
                stack.Push(Expression.Constant(0, typeof(int)));
            }
            else if (code == OpCodes.Ldc_I4_1)
            {
                stack.Push(Expression.Constant(1, typeof(int)));
            }
            else if (code == OpCodes.Ldc_I4_2)
            {
                stack.Push(Expression.Constant(2, typeof(int)));
            }
            else if (code == OpCodes.Ldc_I4_3)
            {
                stack.Push(Expression.Constant(3, typeof(int)));
            }
            else if (code == OpCodes.Ldc_I4_4)
            {
                stack.Push(Expression.Constant(4, typeof(int)));
            }
            else if (code == OpCodes.Ldc_I4_5)
            {
                stack.Push(Expression.Constant(5, typeof(int)));
            }
            else if (code == OpCodes.Ldc_I4_6)
            {
                stack.Push(Expression.Constant(6, typeof(int)));
            }
            else if (code == OpCodes.Ldc_I4_7)
            {
                stack.Push(Expression.Constant(7, typeof(int)));
            }
            else if (code == OpCodes.Ldc_I4_8)
            {
                stack.Push(Expression.Constant(8, typeof(int)));
            }
            else if (code == OpCodes.Sub)
            {
                var right = stack.Pop();
                var left = stack.Pop();
                stack.Push(Expression.Subtract(left, right));
            }
            else if (code == OpCodes.Ret)
            {
            }
            else
            {
                throw new NotSupportedException(code.ToString());
            }
        }

        /// <summary>
        /// Puts the specified instruction onto the Microsoft intermediate language (MSIL) stream followed by the index of the given local variable.
        /// </summary>
        /// <param name="code">The MSIL instruction to be emitted onto the stream.</param>
        /// <param name="localBuilder">A local variable.</param>
        public void Emit(OpCode code, LocalBuilder localBuilder)
        {
            if (code == OpCodes.Stloc)
            {
                Expression valueExpression = stack.Pop();
                var assignExpression = Expression.Assign(localBuilder.Variable, valueExpression);
                expressions.Add(assignExpression);
            }
            else if (code == OpCodes.Ldloc)
            {
                stack.Push(localBuilder.Variable);
            }
            else
            {
                throw new NotSupportedException(code.ToString());
            }
        }

        /// <summary>
        /// Puts the specified instruction and numerical argument onto the Microsoft intermediate language (MSIL) stream of instructions.
        /// </summary>
        /// <param name="code">The MSIL instruction to be put onto the stream.</param>
        /// <param name="arg">The numerical argument pushed onto the stream immediately after the instruction.</param>
        public void Emit(OpCode code, int arg)
        {
            if (code == OpCodes.Ldc_I4)
            {
                stack.Push(Expression.Constant(arg, typeof(int)));
            }
            else if (code == OpCodes.Ldarg)
            {
                stack.Push(parameters[arg]);
            }
            else if (code == OpCodes.Ldloc)
            {
                stack.Push(locals[arg].Variable);
            }
            else if (code == OpCodes.Stloc)
            {
                Expression valueExpression = stack.Pop();
                var assignExpression = Expression.Assign(locals[arg].Variable, valueExpression);
                expressions.Add(assignExpression);
            }
            else
            {
                throw new NotSupportedException(code.ToString());
            }
        }

        /// <summary>
        /// Puts the specified instruction and numerical argument onto the Microsoft intermediate language (MSIL) stream of instructions.
        /// </summary>
        /// <param name="code">The MSIL instruction to be put onto the stream.</param>
        /// <param name="arg">The numerical argument pushed onto the stream immediately after the instruction.</param>
        public void Emit(OpCode code, sbyte arg)
        {
            if (code == OpCodes.Ldc_I4_S)
            {
                stack.Push(Expression.Constant((int)arg, typeof(int)));
            }
            else
            {
                throw new NotSupportedException(code.ToString());
            }
        }

        /// <summary>
        /// Puts the specified instruction and numerical argument onto the Microsoft intermediate language (MSIL) stream of instructions.
        /// </summary>
        /// <param name="code">The MSIL instruction to be put onto the stream.</param>
        /// <param name="arg">The numerical argument pushed onto the stream immediately after the instruction.</param>
        public void Emit(OpCode code, byte arg)
        {
            if (code == OpCodes.Ldloc_S)
            {
                stack.Push(locals[arg].Variable);
            }
            else if (code == OpCodes.Ldarg_S)
            {
                stack.Push(parameters[arg]);
            }
            else if (code == OpCodes.Stloc_S)
            {
                Expression valueExpression = stack.Pop();
                var assignExpression = Expression.Assign(locals[arg].Variable, valueExpression);
                expressions.Add(assignExpression);
            }
            else
            {
                throw new NotSupportedException(code.ToString());
            }
        }

        /// <summary>
        /// Puts the specified instruction onto the Microsoft intermediate language (MSIL) stream followed by the metadata token for the given string.
        /// </summary>
        /// <param name="code">The MSIL instruction to be emitted onto the stream.</param>
        /// <param name="arg">The String to be emitted.</param>
        public void Emit(OpCode code, string arg)
        {
            if (code == OpCodes.Ldstr)
            {
                stack.Push(Expression.Constant(arg, typeof(string)));
            }
            else
            {
                throw new NotSupportedException(code.ToString());
            }
        }

        /// <summary>
        /// Declares a local variable of the specified type.
        /// </summary>
        /// <param name="type">A <see cref="Type"/> object that represents the type of the local variable.</param>
        /// <returns>The declared local variable.</returns>
        public LocalBuilder DeclareLocal(Type type)
        {
            var localBuilder = new LocalBuilder(type, locals.Count);
            locals.Add(localBuilder);
            return localBuilder;
        }

        /// <summary>
        /// Puts the specified instruction onto the Microsoft intermediate language (MSIL) stream followed by the metadata token for the given type.
        /// </summary>
        /// <param name="code">The MSIL instruction to be put onto the stream.</param>
        /// <param name="type">A <see cref="Type"/>.</param>
        public void Emit(OpCode code, Type type)
        {
            if (code == OpCodes.Newarr)
            {
                stack.Push(Expression.NewArrayBounds(type, Pop(1)));
            }
            else if (code == OpCodes.Stelem)
            {
                var value = stack.Pop();
                var index = stack.Pop();
                var array = stack.Pop();
                var arrayAccess = Expression.ArrayAccess(array, index);

                var assignExpression = Expression.Assign(arrayAccess, value);
                expressions.Add(assignExpression);
            }
            else if (code == OpCodes.Castclass)
            {
                stack.Push(Expression.Convert(stack.Pop(), type));
            }
            else if (code == OpCodes.Box)
            {
                stack.Push(Expression.Convert(stack.Pop(), typeof(object)));
            }
            else if (code == OpCodes.Unbox_Any)
            {
                stack.Push(Expression.Convert(stack.Pop(), type));
            }
            else
            {
                throw new NotSupportedException(code.ToString());
            }
        }

        /// <summary>
        /// Puts the specified instruction onto the Microsoft intermediate language (MSIL) stream followed by the metadata token for the given method.
        /// </summary>
        /// <param name="code">The MSIL instruction to be emitted onto the stream.</param>
        /// <param name="methodInfo">A <see cref="MethodInfo"/> representing a method.</param>
        public void Emit(OpCode code, MethodInfo methodInfo)
        {
            if (code == OpCodes.Callvirt || code == OpCodes.Call)
            {
                var parameterCount = methodInfo.GetParameters().Length;
                Expression[] arguments = Pop(parameterCount);

                MethodCallExpression methodCallExpression;

                if (!methodInfo.IsStatic)
                {
                    var instance = stack.Pop();
                    methodCallExpression = Expression.Call(instance, methodInfo, arguments);
                }
                else
                {
                    methodCallExpression = Expression.Call(null, methodInfo, arguments);
                }

                if (methodInfo.ReturnType == typeof(void))
                {
                    expressions.Add(methodCallExpression);
                }
                else
                {
                    stack.Push(methodCallExpression);
                }
            }
            else
            {
                throw new NotSupportedException(code.ToString());
            }
        }

        private Expression[] Pop(int numberOfElements)
        {
            var expressionsToPop = new Expression[numberOfElements];

            for (int i = 0; i < numberOfElements; i++)
            {
                expressionsToPop[i] = stack.Pop();
            }

            return expressionsToPop.Reverse().ToArray();
        }
    }
}