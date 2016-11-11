using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibGit2Sharp;

namespace Cake.GitPackager
{
    /// <summary>
    /// Files packager based on Git log.
    /// </summary>
    public class GitPackagerRunner
    {
        /// <summary>
        /// Copy modified files from Git to a package folder using Git log.
        /// </summary>
        /// <param name="repository">Git repository path.</param>
        /// <param name="mapping">Git folders structure to package folders structure mapping.</param>
        /// <param name="tag">Git tag.</param>
        /// <param name="commit">Git commit SHA hash. It has higher priority than the tag.</param>
        public void Package(
            string repository, 
            IDictionary<string, string> mapping,
            string tag = null,
            string commit = null)
        {
            if (string.IsNullOrEmpty(repository))
            {
                throw new ArgumentException(nameof(repository));
            }

            if (mapping == null)
            {
                throw new ArgumentNullException(nameof(mapping));
            }

            if (string.IsNullOrEmpty(tag) && string.IsNullOrEmpty(commit))
            {
                throw new ArgumentException("Tag or commit SHA is required");
            }

            using (var git = new Repository(repository))
            {
                var lastCommit = FindCommit(git, tag, commit);
                if (string.IsNullOrWhiteSpace(lastCommit))
                {
                    throw new InvalidOperationException("Tag and Commit are not found");
                }

                Logger.Log($"Processing changes till {lastCommit}");
                var filter = new CommitFilter { ExcludeReachableFrom = lastCommit };
                var commits = git.Commits.QueryBy(filter).ToList();
                if (commits.Count == 0)
                {
                    Logger.Log($"No commits found");
                }
                else
                {
                    var newCommit = commits.First();
                    var oldCommit = commits.Count == 1 ? newCommit.Parents.First() : commits.Last();
                    var changes = git.Diff.Compare<TreeChanges>(oldCommit.Tree, newCommit.Tree);
                    CopyFiles(changes, repository, mapping);
                }
            }
        }

        /// <summary>
        /// Copy files to the package folder.
        /// </summary>
        /// <param name="changes">Git changes.</param>
        /// <param name="repository">Git repository path.</param>
        /// <param name="mapping">Git folders structure to package folders structure mapping.</param>
        protected virtual void CopyFiles(TreeChanges changes, string repository, IDictionary<string, string> mapping)
        {
            foreach(var change in changes)
            {
                switch(change.Status)
                {
                    case ChangeKind.Added:
                    case ChangeKind.Modified:
                    case ChangeKind.Renamed:
                        var source = Path.GetDirectoryName(change.Path);
                        var target = mapping.FirstOrDefault(m =>
                        {
                            var dir = m.Key;
                            if (dir.EndsWith(@"\"))
                            {
                                dir = dir.Substring(0, dir.Length - 1);
                            }

                            return source.StartsWith(dir);
                        });

                        if (string.IsNullOrEmpty(target.Value))
                        {
                            continue;
                        }

                        var targetFilename = change.Path.Substring(target.Key.Length);
                        targetFilename = Path.Combine(target.Value, targetFilename);

                        var sourceFilename = Path.Combine(repository, change.Path);
                        if (File.Exists(sourceFilename))
                        {
                            var dir = Path.GetDirectoryName(targetFilename);
                            if (!Directory.Exists(dir))
                            {
                                Directory.CreateDirectory(dir);
                            }

                            File.Copy(sourceFilename, targetFilename, true);

                            Logger.Log($"{change.Path} has been packaged");
                        }
                        else
                        {
                            Logger.Log($"{sourceFilename} is not found");
                        }
                        
                        break;
                }
                
            }
        }

        /// <summary>
        /// Find target Git changest.
        /// </summary>
        /// <param name="git">Repository.</param>
        /// <param name="tag">Git Tag.</param>
        /// <param name="commit">Git commit SHA hash. It has higher priority than the tag.</param>
        /// <returns>Target changeset SHA hash.</returns>
        protected virtual string FindCommit(Repository git, string tag = null, string commit = null)
        {
            string foundCommit = null;

            if (!string.IsNullOrWhiteSpace(tag))
            {
                var info = git.Tags[tag];
                foundCommit = info.Target.Sha;
            }

            if (!string.IsNullOrWhiteSpace(commit))
            {
                foundCommit = commit;
            }

            return foundCommit;
        }
    }
}
