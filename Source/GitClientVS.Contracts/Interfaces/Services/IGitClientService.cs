﻿using System.Collections.Generic;
using System.Threading.Tasks;
using GitClientVS.Contracts.Models.GitClientModels;

namespace GitClientVS.Contracts.Interfaces.Services
{
    public interface IGitClientService
    {
        string Title { get; }
        string Origin { get; }
        Task LoginAsync(string login, string password);
        void Logout();
        Task<IEnumerable<GitRemoteRepository>> GetUserRepositoriesAsync();
        Task<IEnumerable<GitRemoteRepository>> GetAllRepositories();
        Task<GitRemoteRepository> CreateRepositoryAsync(GitRemoteRepository newRepository);
        Task<IEnumerable<GitBranch>> GetBranches(string repoName);
        Task<IEnumerable<GitPullRequest>> GetPullRequests(string repositoryName);
        Task<IEnumerable<GitPullRequest>> GetPullRequests(GitPullRequestStatus gitPullRequestStatus,
            string repositoryName);

        Task<IEnumerable<GitTeam>> GetTeams();

        Task<IEnumerable<GitCommit>> GetPullRequestCommits(string repositoryName, string ownerName, long id);
        Task<IEnumerable<GitCommit>> GetPullRequestCommits(string repositoryName, long id);
        Task<IEnumerable<GitComment>> GetPullRequestComments(string repositoryName, string ownerName, long id);
        Task<IEnumerable<GitComment>> GetPullRequestComments(string repositoryName, long id);

        Task<IEnumerable<GitPullRequest>> GetPullRequests(string repositoryName, string ownerName);

        Task<string> GetPullRequestDiff(string repositoryName, string ownerName, long id);
        Task<string> GetPullRequestDiff(string repositoryName, long id);

        bool IsOriginRepo(GitRemoteRepository gitRemoteRepository);

        Task<GitPullRequest> CreatePullRequest(GitPullRequest gitPullRequest, string repositoryName);
    }
}