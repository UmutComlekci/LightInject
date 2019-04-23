namespace LightInject
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

#if NET452 || NET46 || NETCOREAPP2_0

    /// <summary>
    /// Loads all assemblies from the application base directory that matches the given search pattern.
    /// </summary>
    public class AssemblyLoader : IAssemblyLoader
    {
        /// <summary>
        /// Loads a set of assemblies based on the given <paramref name="searchPattern"/>.
        /// </summary>
        /// <param name="searchPattern">The search pattern to use.</param>
        /// <returns>A list of assemblies based on the given <paramref name="searchPattern"/>.</returns>
        public IEnumerable<Assembly> Load(string searchPattern)
        {
            string directory = Path.GetDirectoryName(new Uri(GetAssemblyCodeBasePath()).LocalPath);
            if (directory != null)
            {
                string[] searchPatterns = searchPattern.Split('|');
                foreach (string file in searchPatterns.SelectMany(sp => Directory.GetFiles(directory, sp)).Where(CanLoad))
                {
                    yield return LoadAssembly(file);
                }
            }
        }

        /// <summary>
        /// Indicates if the current <paramref name="fileName"/> represent a file that can be loaded.
        /// </summary>
        /// <param name="fileName">The name of the target file.</param>
        /// <returns><b>true</b> if the file can be loaded, otherwise <b>false</b>.</returns>
        protected virtual bool CanLoad(string fileName)
        {
            return true;
        }

        /// <summary>
        /// Loads <see cref="Assembly"/> for the file located in <paramref name="filename"/>.
        /// </summary>
        /// <param name="filename">Full path to the file.</param>
        /// <returns><see cref="Assembly"/> of the file.</returns>
        protected virtual Assembly LoadAssembly(string filename)
        {
            return Assembly.LoadFrom(filename);
        }

        /// <summary>
        /// Gets the path where the LightInject assembly is located.
        /// </summary>
        /// <returns>The path where the LightInject assembly is located.</returns>
        protected virtual string GetAssemblyCodeBasePath()
        {
            return typeof(ServiceContainer).Assembly.CodeBase;
        }
    }
#endif

#if NETSTANDARD1_6 || NETSTANDARD2_0
    /// <summary>
    /// Loads all assemblies from the application base directory that matches the given search pattern.
    /// </summary>
    public class AssemblyLoader : IAssemblyLoader
    {
        /// <summary>
        /// Loads a set of assemblies based on the given <paramref name="searchPattern"/>.
        /// </summary>
        /// <param name="searchPattern">The search pattern to use.</param>
        /// <returns>A list of assemblies based on the given <paramref name="searchPattern"/>.</returns>
        public IEnumerable<Assembly> Load(string searchPattern)
        {
            string directory = Path.GetDirectoryName(new Uri(GetAssemblyCodeBasePath()).LocalPath);
            if (directory != null)
            {
                string[] searchPatterns = searchPattern.Split('|');
                foreach (string file in searchPatterns.SelectMany(sp => Directory.GetFiles(directory, sp)).Where(CanLoad))
                {
                    yield return LoadAssembly(file);
                }
            }
        }

        /// <summary>
        /// Indicates if the current <paramref name="fileName"/> represent a file that can be loaded.
        /// </summary>
        /// <param name="fileName">The name of the target file.</param>
        /// <returns><b>true</b> if the file can be loaded, otherwise <b>false</b>.</returns>
        protected virtual bool CanLoad(string fileName)
        {
            return true;
        }

        /// <summary>
        /// Loads <see cref="Assembly"/> for the file located in <paramref name="filename"/>.
        /// </summary>
        /// <param name="filename">Full path to the file.</param>
        /// <returns><see cref="Assembly"/> of the file.</returns>
        protected virtual Assembly LoadAssembly(string filename)
        {
            FileInfo fileInfo = new FileInfo(filename);
            return Assembly.Load(new AssemblyName(fileInfo.Name.Replace(fileInfo.Extension, string.Empty)));
        }

        /// <summary>
        /// Gets the path where the LightInject assembly is located.
        /// </summary>
        /// <returns>The path where the LightInject assembly is located.</returns>
        protected virtual string GetAssemblyCodeBasePath()
        {
            return typeof(ServiceContainer).GetTypeInfo().Assembly.CodeBase;
        }
    }
#endif
}