namespace LightInject
{
   /// <summary>
    /// Extends the <see cref="IEmitter"/> interface with a set of methods
    /// that optimizes and simplifies emitting MSIL instructions.
    /// </summary>
    public static class EmitterExtensions
    {
        /// <summary>
        /// Performs a cast or unbox operation if the current <see cref="IEmitter.StackType"/> is
        /// different from the given <paramref name="type"/>.
        /// </summary>
        /// <param name="emitter">The target <see cref="IEmitter"/>.</param>
        /// <param name="type">The requested stack type.</param>
        public static void UnboxOrCast(this IEmitter emitter, Type type)
        {
            if (!type.GetTypeInfo().IsAssignableFrom(emitter.StackType.GetTypeInfo()))
            {
                emitter.Emit(type.GetTypeInfo().IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, type);
            }
        }

        /// <summary>
        /// Pushes a constant value onto the evaluation stack.
        /// </summary>
        /// <param name="emitter">The target <see cref="IEmitter"/>.</param>
        /// <param name="index">The index of the constant value to be pushed onto the stack.</param>
        /// <param name="type">The requested stack type.</param>
        public static void PushConstant(this IEmitter emitter, int index, Type type)
        {
            emitter.PushConstant(index);
            emitter.UnboxOrCast(type);
        }

        /// <summary>
        /// Pushes a constant value onto the evaluation stack as a object reference.
        /// </summary>
        /// <param name="emitter">The target <see cref="IEmitter"/>.</param>
        /// <param name="index">The index of the constant value to be pushed onto the stack.</param>
        public static void PushConstant(this IEmitter emitter, int index)
        {
            emitter.PushArgument(0);
            emitter.Push(index);
            emitter.PushArrayElement();
        }

        /// <summary>
        /// Pushes the element containing an object reference at a specified index onto the stack.
        /// </summary>
        /// <param name="emitter">The target <see cref="IEmitter"/>.</param>
        public static void PushArrayElement(this IEmitter emitter)
        {
            emitter.Emit(OpCodes.Ldelem_Ref);
        }

        /// <summary>
        /// Pushes the arguments associated with a service request onto the stack.
        /// The arguments are found as an array in the last element of the constants array
        /// that is passed into the dynamic method.
        /// </summary>
        /// <param name="emitter">The target <see cref="IEmitter"/>.</param>
        /// <param name="parameters">A list of <see cref="ParameterInfo"/> instances that
        /// represent the arguments to be pushed onto the stack.</param>
        public static void PushArguments(this IEmitter emitter, ParameterInfo[] parameters)
        {
            var argumentArray = emitter.DeclareLocal(typeof(object[]));
            emitter.Emit(OpCodes.Ldarg_0);
            emitter.Emit(OpCodes.Ldarg_0);
            emitter.Emit(OpCodes.Ldlen);
            emitter.Emit(OpCodes.Conv_I4);
            emitter.Emit(OpCodes.Ldc_I4_1);
            emitter.Emit(OpCodes.Sub);
            emitter.Emit(OpCodes.Ldelem_Ref);
            emitter.Emit(OpCodes.Castclass, typeof(object[]));
            emitter.Emit(OpCodes.Stloc, argumentArray);

            for (int i = 0; i < parameters.Length; i++)
            {
                emitter.Emit(OpCodes.Ldloc, argumentArray);
                emitter.Emit(OpCodes.Ldc_I4, i);
                emitter.Emit(OpCodes.Ldelem_Ref);
                emitter.Emit(
                    parameters[i].ParameterType.GetTypeInfo().IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass,
                    parameters[i].ParameterType);
            }
        }

        /// <summary>
        /// Calls a late-bound method on an object, pushing the return value onto the stack.
        /// </summary>
        /// <param name="emitter">The target <see cref="IEmitter"/>.</param>
        /// <param name="methodInfo">The <see cref="MethodInfo"/> that represents the method to be called.</param>
        public static void Call(this IEmitter emitter, MethodInfo methodInfo)
        {
            emitter.Emit(OpCodes.Callvirt, methodInfo);
        }

        /// <summary>
        /// Pushes a new instance onto the stack.
        /// </summary>
        /// <param name="emitter">The target <see cref="IEmitter"/>.</param>
        /// <param name="constructorInfo">The <see cref="ConstructionInfo"/> that represent the object to be created.</param>
        public static void New(this IEmitter emitter, ConstructorInfo constructorInfo)
        {
            emitter.Emit(OpCodes.Newobj, constructorInfo);
        }

        /// <summary>
        /// Pushes the given <paramref name="localBuilder"/> onto the stack.
        /// </summary>
        /// <param name="emitter">The target <see cref="IEmitter"/>.</param>
        /// <param name="localBuilder">The <see cref="LocalBuilder"/> to be pushed onto the stack.</param>
        public static void Push(this IEmitter emitter, LocalBuilder localBuilder)
        {
            int index = localBuilder.LocalIndex;
            switch (index)
            {
                case 0:
                    emitter.Emit(OpCodes.Ldloc_0);
                    return;
                case 1:
                    emitter.Emit(OpCodes.Ldloc_1);
                    return;
                case 2:
                    emitter.Emit(OpCodes.Ldloc_2);
                    return;
                case 3:
                    emitter.Emit(OpCodes.Ldloc_3);
                    return;
            }

            if (index <= 255)
            {
                emitter.Emit(OpCodes.Ldloc_S, (byte)index);
            }
            else
            {
                emitter.Emit(OpCodes.Ldloc, index);
            }
        }

        /// <summary>
        /// Pushes an argument with the given <paramref name="index"/> onto the stack.
        /// </summary>
        /// <param name="emitter">The target <see cref="IEmitter"/>.</param>
        /// <param name="index">The index of the argument to be pushed onto the stack.</param>
        public static void PushArgument(this IEmitter emitter, int index)
        {
            switch (index)
            {
                case 0:
                    emitter.Emit(OpCodes.Ldarg_0);
                    return;
                case 1:
                    emitter.Emit(OpCodes.Ldarg_1);
                    return;
                case 2:
                    emitter.Emit(OpCodes.Ldarg_2);
                    return;
                case 3:
                    emitter.Emit(OpCodes.Ldarg_3);
                    return;
            }

            if (index <= 255)
            {
                emitter.Emit(OpCodes.Ldarg_S, (byte)index);
            }
            else
            {
                emitter.Emit(OpCodes.Ldarg, index);
            }
        }

        /// <summary>
        /// Stores the value currently on top of the stack in the given <paramref name="localBuilder"/>.
        /// </summary>
        /// <param name="emitter">The target <see cref="IEmitter"/>.</param>
        /// <param name="localBuilder">The <see cref="LocalBuilder"/> for which the value is to be stored.</param>
        public static void Store(this IEmitter emitter, LocalBuilder localBuilder)
        {
            int index = localBuilder.LocalIndex;
            switch (index)
            {
                case 0:
                    emitter.Emit(OpCodes.Stloc_0);
                    return;
                case 1:
                    emitter.Emit(OpCodes.Stloc_1);
                    return;
                case 2:
                    emitter.Emit(OpCodes.Stloc_2);
                    return;
                case 3:
                    emitter.Emit(OpCodes.Stloc_3);
                    return;
            }

            if (index <= 255)
            {
                emitter.Emit(OpCodes.Stloc_S, (byte)index);
            }
            else
            {
                emitter.Emit(OpCodes.Stloc, index);
            }
        }

        /// <summary>
        /// Pushes a new array of the given <paramref name="elementType"/> onto the stack.
        /// </summary>
        /// <param name="emitter">The target <see cref="IEmitter"/>.</param>
        /// <param name="elementType">The element <see cref="Type"/> of the new array.</param>
        public static void PushNewArray(this IEmitter emitter, Type elementType)
        {
            emitter.Emit(OpCodes.Newarr, elementType);
        }

        /// <summary>
        /// Pushes an <see cref="int"/> value onto the stack.
        /// </summary>
        /// <param name="emitter">The target <see cref="IEmitter"/>.</param>
        /// <param name="value">The <see cref="int"/> value to be pushed onto the stack.</param>
        public static void Push(this IEmitter emitter, int value)
        {
            switch (value)
            {
                case 0:
                    emitter.Emit(OpCodes.Ldc_I4_0);
                    return;
                case 1:
                    emitter.Emit(OpCodes.Ldc_I4_1);
                    return;
                case 2:
                    emitter.Emit(OpCodes.Ldc_I4_2);
                    return;
                case 3:
                    emitter.Emit(OpCodes.Ldc_I4_3);
                    return;
                case 4:
                    emitter.Emit(OpCodes.Ldc_I4_4);
                    return;
                case 5:
                    emitter.Emit(OpCodes.Ldc_I4_5);
                    return;
                case 6:
                    emitter.Emit(OpCodes.Ldc_I4_6);
                    return;
                case 7:
                    emitter.Emit(OpCodes.Ldc_I4_7);
                    return;
                case 8:
                    emitter.Emit(OpCodes.Ldc_I4_8);
                    return;
            }

            if (value > -129 && value < 128)
            {
                emitter.Emit(OpCodes.Ldc_I4_S, (sbyte)value);
            }
            else
            {
                emitter.Emit(OpCodes.Ldc_I4, value);
            }
        }

        /// <summary>
        /// Performs a cast of the value currently on top of the stack to the given <paramref name="type"/>.
        /// </summary>
        /// <param name="emitter">The target <see cref="IEmitter"/>.</param>
        /// <param name="type">The <see cref="Type"/> for which the value will be casted into.</param>
        public static void Cast(this IEmitter emitter, Type type)
        {
            emitter.Emit(OpCodes.Castclass, type);
        }

        /// <summary>
        /// Returns from the current method.
        /// </summary>
        /// <param name="emitter">The target <see cref="IEmitter"/>.</param>
        public static void Return(this IEmitter emitter)
        {
            emitter.Emit(OpCodes.Ret);
        }
    }
}