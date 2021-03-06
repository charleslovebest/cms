﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Configuration;

namespace SSCMS.Web.Controllers.Admin.Cms.Channels
{
    public partial class ChannelsLayerTaxisController
    {
        [HttpPost, Route(Route)]
        public async Task<ActionResult<List<int>>> Submit([FromBody] SubmitRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId,
                Types.SitePermissions.Channels))
            {
                return Unauthorized();
            }

            var site = await _siteRepository.GetAsync(request.SiteId);
            if (site == null) return NotFound();

            var expendedChannelIds = new List<int>
            {
                request.SiteId
            };
            foreach (var channelId in request.ChannelIds)
            {
                for (var num = 0; num < request.Taxis; num++)
                {
                    var channel = await _channelRepository.GetAsync(channelId);
                    if (!expendedChannelIds.Contains(channel.ParentId))
                    {
                        expendedChannelIds.Add(channel.ParentId);
                    }
                    if (request.IsUp)
                    {
                        await _channelRepository.UpdateTaxisUpAsync(request.SiteId, channelId, channel.ParentId, channel.Taxis);
                    }
                    else
                    {
                        await _channelRepository.UpdateTaxisDownAsync(request.SiteId, channelId, channel.ParentId, channel.Taxis);
                    }
                }

                await _authManager.AddSiteLogAsync(request.SiteId, channelId, 0, "栏目排序" + (request.IsUp ? "上升" : "下降"), $"栏目:{_channelRepository.GetChannelNameAsync(request.SiteId, channelId)}");
            }

            return expendedChannelIds;
        }
    }
}