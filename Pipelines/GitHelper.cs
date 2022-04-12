using System;
using System.Collections.Generic;
using AzureDevops.Base;
using Microsoft.TeamFoundation.SourceControl.WebApi;

namespace AzureDevops.Pipelines
{
    public class GitHelper: PipelineBase
    {
        public IEnumerable<TfvcBranch> ListBranches(Guid projectId)
        {
            var connection = GetConnection(projectId);
            var tfvcClient = connection.GetClient<TfvcHttpClient>();

            var branches = tfvcClient.GetBranchesAsync(includeParent: true, includeChildren: true).Result;

            foreach (TfvcBranch branch in branches)
            {
                Console.WriteLine("{0} ({2}): {1}", branch.Path, branch.Description ?? "<no description>", branch.Owner.DisplayName);
            }

            if (branches.Count == 0)
            {
                Console.WriteLine("No branches found.");
            }

            return branches;
        }
    }
}
