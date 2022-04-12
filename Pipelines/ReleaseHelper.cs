using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Contracts;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Clients;
using AzureDevops.Base;

namespace AzureDevops.Pipelines
{
    public class ReleaseHelper: PipelineBase
    {
        public void CreateReleaseWithDefaultArtifacts(Guid projectId, string projectName, int definitionId, string description)
        {
            var releaseStartMetaData = new ReleaseStartMetadata();
            releaseStartMetaData.DefinitionId = definitionId;
            releaseStartMetaData.Description = description;

            var connection = GetConnection(projectId);
            var releaseClient = connection.GetClient<ReleaseHttpClient>();
            var output = releaseClient.CreateReleaseAsync(project: projectName, releaseStartMetadata: releaseStartMetaData).Result;             
        }

        public void CreateReleaseWithArtifacts(Guid projectId, string projectName, int definitionId, string description, ArtifactMetadata[] artifacts)
        {
            var releaseStartMetaData = new ReleaseStartMetadata();
            releaseStartMetaData.DefinitionId = definitionId;
            releaseStartMetaData.Description = description;

            releaseStartMetaData.Artifacts = artifacts;

            var connection = GetConnection(projectId);
            var releaseClient = connection.GetClient<ReleaseHttpClient>();
            var output = releaseClient.CreateReleaseAsync(project: projectName, releaseStartMetadata: releaseStartMetaData).Result;             
        }

        public ReleaseDefinition GetRelease(Guid projectId, string projectName, int definitionId)
        {
            var connection = GetConnection(projectId);
            var releaseClient = connection.GetClient<ReleaseHttpClient>();

            var release = releaseClient.GetReleaseDefinitionAsync(project: projectName, definitionId).Result;

            release.Artifacts.ForEach(x => Console.WriteLine(x.Alias));
            return release;
        }

        public List<ReleaseDefinition> ListAllReleaseDefinitions(Guid projectId, string projectName)
        {
            var connection = GetConnection(projectId);
            var releaseClient = connection.GetClient<ReleaseHttpClient>();
            var releaseDefinitions = releaseClient.GetReleaseDefinitionsAsync(project: projectName).Result;

            return releaseDefinitions;
        }

        public void CreateRelease(Guid projectId, string projectName, int definitionId, string branchName, IEnumerable<Tuple<string, int>> buildNameToIdTuple) {
            var releaseDef = GetRelease(projectId, projectName, definitionId);
            
            var artifacts = new List<ArtifactMetadata>();
            foreach (var b in buildNameToIdTuple)
            {
                var buildDefName = releaseDef.Artifacts.FirstOrDefault(x => (x as dynamic).DefinitionReference["definition"].Name == b.Item1);
                if (buildDefName != null) {
                    artifacts.Add(new ArtifactMetadata { Alias = buildDefName.Alias, InstanceReference = new BuildVersion { Id = b.Item2.ToString() }});
                }
            }

            var releaseStartMetaData = new ReleaseStartMetadata();
            releaseStartMetaData.DefinitionId = definitionId;


            releaseStartMetaData.Artifacts = artifacts;

            var connection = GetConnection(projectId);
            var releaseClient = connection.GetClient<ReleaseHttpClient>();
            var output = releaseClient.CreateReleaseAsync(project: projectName, releaseStartMetadata: releaseStartMetaData).Result;             
        }
    }
}
