using System;
using Microsoft.VisualStudio.Services.WebApi;
using System.Collections.Generic;
using Microsoft.TeamFoundation.Build.WebApi;
using System.Linq;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Contracts;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;
using Microsoft.VisualStudio.Services.Client;
using AzureDevops.Base;

namespace AzureDevops.Pipelines
{
    public class BuildHelper: PipelineBase
    {
        public void TriggerBuild(int buildDefinitionId, string buildName, Guid projectId, string branchName, string agentPoolName)
        {
            var buildDef = new BuildDefinitionReference() {
                Id = buildDefinitionId,
                Name = buildName
            };
            
            this.TriggerBuild(buildDef, projectId, branchName, agentPoolName);
        }

        public void TriggerBuild(BuildDefinitionReference definition, Guid projectId, string branchName, string agentPoolName)
        {
            var project = GetProjectReference(projectId);
            var connection = GetConnection(projectId);
            var buildClient = connection.GetClient<BuildHttpClient>();

            var agentSpecification = new Microsoft.TeamFoundation.Build.WebApi.AgentSpecification();
            agentSpecification.Identifier = agentPoolName;

            buildClient.QueueBuildAsync(new Build() {
                AgentSpecification = agentSpecification,
                Definition = new DefinitionReference() {
                    Id = definition.Id,
                    Name = definition.Name
                },
                SourceBranch = branchName,
                Project = project
            });
        }

        public Build GetBuildDefinition(Guid projectId, string projectName, int buildDefinitionId)
        {
            var connection = GetConnection(projectId);
            var buildClient = connection.GetClient<BuildHttpClient>();
            var buildDefinition = buildClient.GetBuildAsync(projectName, buildDefinitionId).Result;
            
            return buildDefinition;
        }

        //Create tuple for the most recent builds in given branch: Tuple<BuildName, BuildId> 
        public IEnumerable<Tuple<string, int>> GetMostRecentArtifactFromBuilds(Guid projectId, string projectName, string branch)
        {
            var buildDefinitions = ListBuildDefinitions(projectId, projectName);
            var connection = GetConnection(projectId);
            var buildClient = connection.GetClient<BuildHttpClient>();

            foreach(var buildD in buildDefinitions)
            {
                var builds = buildClient.GetBuildsAsync2(projectId, new int[] { buildD.Id}).Result;
                
                Console.WriteLine("{0} builds for {1}", builds.Count, buildD.Name);
                var build = builds.OrderByDescending(x => x.QueueTime).FirstOrDefault(x => x.SourceBranch == $"refs/heads/{branch}");
                if (build == null) continue;
                yield return Tuple.Create<string, int>(build.Definition.Name, build.Id);
            }
        }

        public IList<ArtifactMetadata> GetArtifactForBuild(Guid projectId, string projectName, int buildID)
        {
            var project = GetProjectReference(projectId);
            var creds = new VssClientCredentials();
            
            var connection = GetConnection(projectId);
            var buildClient = connection.GetClient<BuildHttpClient>();

            var artifact = buildClient.GetArtifactsAsync(projectName, buildID).Result;
            Console.WriteLine("{0} builds for {1}", artifact.Count, artifact[0].Source);
            
            var releaseStartMetaData = new ReleaseStartMetadata();

            return releaseStartMetaData.Artifacts;
        }

        public IEnumerable<BuildDefinitionReference> GetBuildDefinitionsFromFolder(Guid projectId, string projectName, string path)
        {
            var connection = GetConnection(projectId);
            var buildClient = connection.GetClient<BuildHttpClient>();

            var buildDefinitions = new List<BuildDefinitionReference>();

            string continuationToken = null;
            do
            {
                IPagedList<BuildDefinitionReference> buildDefinitionsPage = buildClient.GetDefinitionsAsync2(
                    project: projectName, continuationToken: continuationToken).Result;

                buildDefinitions.AddRange(buildDefinitionsPage);

                continuationToken = buildDefinitionsPage.ContinuationToken;
            }
            while (!String.IsNullOrEmpty(continuationToken));

            foreach (BuildDefinitionReference definition in buildDefinitions)
            {
                if (definition.Path.Contains(path)) {
                    Console.WriteLine("{0} {1}", definition.Id.ToString().PadLeft(6), definition.Name);
                    yield return definition;
                }
            }
        }

        public IEnumerable<BuildDefinitionReference> ListBuildDefinitions(Guid projectId, string projectName)
        {
            var connection = GetConnection(projectId);
            var buildClient = connection.GetClient<BuildHttpClient>();

            var buildDefinitions = new List<BuildDefinitionReference>();

            string continuationToken = null;
            do
            {
                IPagedList<BuildDefinitionReference> buildDefinitionsPage = buildClient.GetDefinitionsAsync2(
                    project: projectName, 
                    continuationToken: continuationToken).Result;

                buildDefinitions.AddRange(buildDefinitionsPage);

                continuationToken = buildDefinitionsPage.ContinuationToken;
            } while (!String.IsNullOrEmpty(continuationToken));

            return buildDefinitions;
        }
    }
}
