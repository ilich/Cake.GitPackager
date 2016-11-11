using System;
using System.Collections.Generic;
using Cake.Core;
using Cake.Core.Annotations;

namespace Cake.GitPackager
{
    /// <summary>
    /// Cake AddIn which copies files based on Git log.
    /// <code>
    /// #addin Cake.GitPackager
    /// </code>
    /// </summary>
    [CakeAliasCategory("Git")]
    public static class GitPackagerExtensions
    {
        /// <summary>
        /// Copy files from Git repository to the package folder using Git log and directories mapping.
        /// </summary>
        /// <param name="context">Cake context.</param>
        /// <param name="repository">Git repository path.</param>
        /// <param name="mapping">Git folders structure to package folders structure mapping.</param>
        /// <param name="tag">Git tag.</param>
        /// <param name="commit">Git commit SHA hash. It has higher priority than the tag.</param>
        /// <example>
        /// <code>
        /// GitPackager(
        ///     "C:\project\sample",
        ///     new Dictionary<string, string>()
        ///         {
        ///             { @"src\Views\", @"C:\project\sample\Views\" }
        ///         },
        ///     "1.0.0",
        ///     null
        /// );
        /// </code>
        /// </example>
        [CakeMethodAlias]
        public static void GitPackager(
            this ICakeContext context,
            string repository,
            IDictionary<string, string> mapping,
            string tag = null,
            string commit = null)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            Logger.LogEngine = context.Log;

            var runner = new GitPackagerRunner();
            runner.Package(repository, mapping, tag, commit);
        }

        static void Main(string[] args)
        {
            var runner = new GitPackagerRunner();
            runner.Package(
                @"C:\Projects\kortext\Kortext-Store",
                new Dictionary<string, string>
                {
                    [@"src\Presentation\Nop.Web\Themes\Kortext\"] = @"C:\Projects\temp\kortext\Themes\Kortext\",
                    [@"src\Presentation\Nop.Web\Content\"] = @"C:\Projects\temp\kortext\Content\",
                    [@"src\Presentation\Nop.Web\Scripts\"] = @"C:\Projects\temp\kortext\Scripts\",
                    [@"src\Presentation\Nop.Web\Views\"] = @"C:\Projects\temp\kortext\Views\",
                    [@"src\Presentation\Nop.Web\Administration\Content\"] = @"C:\Projects\temp\kortext\Administration\Content\",
                    [@"src\Presentation\Nop.Web\Administration\Views\"] = @"C:\Projects\temp\kortext\Administration\Views\"
                },
                tag: "1.1.5");
                //commit: "3856e5bf60fd426c22b35a847ba27648edced2b8");
        }
    }
}
