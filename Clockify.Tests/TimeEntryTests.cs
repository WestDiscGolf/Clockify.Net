﻿using System;
using System.Threading.Tasks;
using Clockify.Net;
using Clockify.Net.Models.Tags;
using Clockify.Net.Models.TimeEntries;
using Clockify.Net.Models.Workspaces;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;

namespace Clockify.Tests {
	public class TimeEntryTests {
		private readonly ClockifyClient _client;
		private string _workspaceId;

		public TimeEntryTests() {
			_client = new ClockifyClient();
		}

		[SetUp]
		public async Task Setup() {
			var workspaceResponse = await _client.CreateWorkspaceAsync(new WorkspaceRequest { Name = "TimeEntryWorkspace" });
			workspaceResponse.IsSuccessful.Should().BeTrue();
			_workspaceId = workspaceResponse.Data.Id;
		}

		[TearDown]
		public async Task Cleanup() {
			var workspaceResponse = await _client.DeleteWorkspaceAsync(_workspaceId);
			workspaceResponse.IsSuccessful.Should().BeTrue();
		}

		[Test]
		public async Task FindAllTagsOnWorkspaceAsync_ShouldReturnTagsList() {
			var response = await _client.FindAllTagsOnWorkspaceAsync(_workspaceId);
			response.IsSuccessful.Should().BeTrue();
		}

		[Test]
		public async Task CreateTimeEntryAsync_ShouldCreteTimeEntryAndReturnTimeEntryDtoImpl() {
			var now = DateTimeOffset.UtcNow;
			var timeEntryRequest = new TimeEntryRequest {
				Start = now,
			};
			var createResult = await _client.CreateTimeEntryAsync(_workspaceId, timeEntryRequest);
			createResult.IsSuccessful.Should().BeTrue();
			createResult.Data.Should().NotBeNull();
			createResult.Data.TimeInterval.Start.Should().BeCloseTo(now, 1.Seconds());
		}

		[Test]
		public async Task CreateTimeEntryAsync_NullStart_ShouldThrowArgumentException() {
			var timeEntryRequest = new TimeEntryRequest {
				Start = null,
			};
			Func<Task> create = () => _client.CreateTimeEntryAsync(_workspaceId, timeEntryRequest);
			await create.Should().ThrowAsync<ArgumentException>()
				.WithMessage($"Argument cannot be null. (Parameter '{nameof(TimeEntryRequest.Start)}')");
		}

		[Test]
		public async Task GetTimeEntryFromWorkspaceAsync_ShouldReturnTimeEntryDtoImpl() {
			var now = DateTimeOffset.UtcNow;
			var timeEntryRequest = new TimeEntryRequest {
				Start = now,
			};
			var createResult = await _client.CreateTimeEntryAsync(_workspaceId, timeEntryRequest);
			createResult.IsSuccessful.Should().BeTrue();
			createResult.Data.Should().NotBeNull();
			var timeEntryId = createResult.Data.Id;

			var getResult = await _client.GetTimeEntryAsync(_workspaceId, timeEntryId);
			getResult.IsSuccessful.Should().BeTrue();
			getResult.Data.Should().NotBeNull();
			getResult.Data.Should().BeEquivalentTo(createResult.Data);
		}

		[Test]
		public async Task UpdateTimeEntryAsync_ShouldUpdateAndTimeEntryDtoImpl() {
			var now = DateTimeOffset.UtcNow;
			var timeEntryRequest = new TimeEntryRequest {
				Start = now,
			};
			var createResult = await _client.CreateTimeEntryAsync(_workspaceId, timeEntryRequest);
			createResult.IsSuccessful.Should().BeTrue();
			createResult.Data.Should().NotBeNull();
			var timeEntryId = createResult.Data.Id;

			var updateTimeEntryRequest = new UpdateTimeEntryRequest {
				Start = now.AddSeconds(-1),
				Billable = true,
			};
			var updateResult = await _client.UpdateTimeEntryAsync(_workspaceId, timeEntryId, updateTimeEntryRequest);
			updateResult.IsSuccessful.Should().BeTrue();
		}

		[Test]
		public async Task UpdateTimeEntryAsync_NullStart_ShouldThrowArgumentException() {
			var updateTimeEntryRequest = new UpdateTimeEntryRequest() {
				Start = null,
			};
			Func<Task> create = () => _client.UpdateTimeEntryAsync(_workspaceId, "", updateTimeEntryRequest);
			await create.Should().ThrowAsync<ArgumentException>()
				.WithMessage($"Argument cannot be null. (Parameter '{nameof(TimeEntryRequest.Start)}')");
		}

		[Test]
		public async Task UpdateTimeEntryAsync_NullBillable_ShouldThrowArgumentException() {
			var updateTimeEntryRequest = new UpdateTimeEntryRequest() {
				Start = DateTimeOffset.UtcNow,
				Billable = null
			};
			Func<Task> create = () => _client.UpdateTimeEntryAsync(_workspaceId, "", updateTimeEntryRequest);
			await create.Should().ThrowAsync<ArgumentException>()
				.WithMessage($"Argument cannot be null. (Parameter '{nameof(TimeEntryRequest.Billable)}')");
		}

		[Test]
		public async Task DeleteTimeEntryAsync_ShouldDeleteTimeEntry() {
			var now = DateTimeOffset.UtcNow;
			var timeEntryRequest = new TimeEntryRequest {
				Start = now,
			};
			var createResult = await _client.CreateTimeEntryAsync(_workspaceId, timeEntryRequest);
			createResult.IsSuccessful.Should().BeTrue();
			var timeEntryId = createResult.Data.Id;

			var deleteResult = await _client.DeleteTimeEntryAsync(_workspaceId, timeEntryId);
			deleteResult.IsSuccessful.Should().BeTrue();
		}

		[Test]
		public async Task FindAllTimeEntriesForUserAsync_ShouldReturnTimeEntryDtoImplList() {
			var now = DateTimeOffset.UtcNow;
			var timeEntryRequest = new TimeEntryRequest {
				Start = now,
			};
			var createResult = await _client.CreateTimeEntryAsync(_workspaceId, timeEntryRequest);
			createResult.IsSuccessful.Should().BeTrue();

			var userResponse = await _client.GetCurrentUserAsync();
			userResponse.IsSuccessful.Should().BeTrue();


			var response = await _client.FindAllTimeEntriesForUserAsync(_workspaceId, userResponse.Data.Id);
			response.IsSuccessful.Should().BeTrue();
			response.Data.Should().ContainEquivalentOf(createResult.Data);
		}
	}
}