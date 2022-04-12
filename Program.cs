using System;
using AzureDevops.Pipelines;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Contracts;

namespace AzureDevops
{
    public class Program
    {
        public static string CollectionUrl = "<collectionUrl>";
        public static string AgentPoolName = "<agentPoolName>";
        public static string ProjectName = "<projectName>";
        public static Guid ProjectId = new Guid("<project Guid>");

        static void Main(string[] args)
        {
            var buildHelper = new BuildHelper();
            var releaseHelper = new ReleaseHelper();
            
            PrintAllBuilds(buildHelper, releaseHelper);
            PrintAllReleases(buildHelper, releaseHelper);
            TriggerBuild(buildHelper, releaseHelper);
            TriggerReleaseWithDefaultArtifacts(buildHelper, releaseHelper);
            TriggerReleaseWithArtifacts(buildHelper, releaseHelper);
        }

        private static void PrintAllReleases(BuildHelper buildHelper, ReleaseHelper releaseHelper)
        {
            Console.WriteLine("Releases: ");
            var releases = releaseHelper.ListAllReleaseDefinitions(ProjectId, ProjectName);

            foreach (var releaseDefinition in releases)
            {
                Console.WriteLine("{0} {1}", releaseDefinition.Id.ToString().PadLeft(6), releaseDefinition.Name);
            }
        }

        private static void PrintAllBuilds(BuildHelper buildHelper, ReleaseHelper releaseHelper)
        {
            Console.WriteLine("Builds: ");
            var buildDefinitions = buildHelper.ListBuildDefinitions(ProjectId, ProjectName);

            foreach (var buildDef in buildDefinitions)
            {
                Console.WriteLine("{0} {1}", buildDef.Id.ToString().PadLeft(6), buildDef.Name);
            }
        }

        private static void TriggerBuild(BuildHelper buildHelper, ReleaseHelper releaseHelper)
        {
            var buildDefinitionId = 53256;
            var buildName = "develop_Build";
            var branchName = "develop";
            var agentPoolName = "Build_Pool";

            buildHelper.TriggerBuild(buildDefinitionId, buildName, ProjectId, branchName, agentPoolName);
        }

        private static void TriggerReleaseWithDefaultArtifacts(BuildHelper buildHelper, ReleaseHelper releaseHelper)
        {
            var releasePipelineId = 336;
            var releaseComment = "This is the first release";

            releaseHelper.CreateReleaseWithDefaultArtifacts(ProjectId, ProjectName, releasePipelineId, releaseComment);
        }

        private static void TriggerReleaseWithArtifacts(BuildHelper buildHelper, ReleaseHelper releaseHelper)
        {
            var releasePipelineId = 336;
            var releaseComment = "This is the first release";

            var art1 = new ArtifactMetadata { Alias = "<#1 alias for Build>", InstanceReference = new BuildVersion { Id = "752" } };
            var art2 = new ArtifactMetadata { Alias = "<#2 alias for Build>", InstanceReference = new BuildVersion { Id = "511" } };

            releaseHelper.CreateReleaseWithArtifacts(ProjectId, ProjectName, releasePipelineId, releaseComment, new  ArtifactMetadata[] { art1, art2});
        }

        private static void GetMostRecentArtifactFromBuilds(BuildHelper buildHelper, ReleaseHelper releaseHelper)
        {
            var releasePipelineId = 336;
            var releaseComment = "This is the first release";

            var art1 = new ArtifactMetadata { Alias = "<#1 alias for Build>", InstanceReference = new BuildVersion { Id = "752" } };
            var art2 = new ArtifactMetadata { Alias = "<#2 alias for Build>", InstanceReference = new BuildVersion { Id = "511" } };

            releaseHelper.CreateReleaseWithArtifacts(ProjectId, ProjectName, releasePipelineId, releaseComment, new  ArtifactMetadata[] { art1, art2});
        }

        private static void GetRelease(BuildHelper buildHelper, ReleaseHelper releaseHelper)
        {
            var releasePipelineId = 336;
            releaseHelper.GetRelease(ProjectId, ProjectName, releasePipelineId);
        }

        private static void GetBuild(BuildHelper buildHelper, ReleaseHelper releaseHelper)
        {
            var buildDefinitionId = 3463478;
            buildHelper.GetBuildDefinition(ProjectId, ProjectName, buildDefinitionId);
        }

        private static void TriggerAllBuildsFromFolder(BuildHelper buildHelper, ReleaseHelper releaseHelper)
        {
            var projectName = "Test";
            string branchName = "<branchName>";

            var definitions = buildHelper.GetBuildDefinitionsFromFolder(ProjectId, projectName, "\\Folder");

            foreach (var def in definitions)
            {
                buildHelper.TriggerBuild(def, ProjectId, branchName, AgentPoolName);
            }
        }
    }
}
