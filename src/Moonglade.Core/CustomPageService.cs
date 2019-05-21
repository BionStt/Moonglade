﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Edi.Practice.RequestResponseModel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moonglade.Data.Entities;
using Moonglade.Data.Infrastructure;
using Moonglade.Model;
using Moonglade.Model.Settings;

namespace Moonglade.Core
{
    public class CustomPageService : MoongladeService
    {
        private readonly IRepository<CustomPageEntity> _customPageRepository;

        public CustomPageService(
            ILogger<CustomPageService> logger,
            IOptions<AppSettings> settings,
            IRepository<CustomPageEntity> customPageRepository) : base(logger, settings)
        {
            _customPageRepository = customPageRepository;
        }

        public async Task<Response<CustomPage>> GetPageAsync(Guid pageId)
        {
            try
            {
                var entity = await _customPageRepository.GetAsync(pageId);
                var item = EntityToCustomPage(entity);
                return new SuccessResponse<CustomPage>(item);
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Error {nameof(GetPageAsync)}");
                return new FailedResponse<CustomPage>((int)ResponseFailureCode.GeneralException, e.Message);
            }
        }

        public async Task<Response<CustomPage>> GetPageAsync(string routeName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(routeName))
                {
                    throw new ArgumentNullException(nameof(routeName));
                }

                var loweredRouteName = routeName.ToLower();
                var entity = await _customPageRepository.GetAsync(p => p.RouteName == loweredRouteName);
                var item = EntityToCustomPage(entity);
                return new SuccessResponse<CustomPage>(item);
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Error {nameof(GetPageAsync)}");
                return new FailedResponse<CustomPage>((int)ResponseFailureCode.GeneralException, e.Message);
            }
        }

        public async Task<Response<IReadOnlyList<CustomPageMetaData>>> GetPagesMetaDataListAsync()
        {
            try
            {
                var list = await _customPageRepository.SelectAsync(page => new CustomPageMetaData
                {
                    Id = page.Id,
                    CreateOnUtc = page.CreateOnUtc,
                    RouteName = page.RouteName,
                    Title = page.Title
                });

                return new SuccessResponse<IReadOnlyList<CustomPageMetaData>>(list);
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Error {nameof(GetPagesMetaDataListAsync)}");
                return new FailedResponse<IReadOnlyList<CustomPageMetaData>>((int)ResponseFailureCode.GeneralException, e.Message);
            }
        }

        public async Task<Response<Guid>> CreatePageAsync(CreateEditCustomPageRequest request)
        {
            try
            {
                var uid = Guid.NewGuid();
                var customPage = new CustomPageEntity
                {
                    Id = uid,
                    Title = request.Title.Trim(),
                    RouteName = request.RouteName.ToLower().Trim(),
                    CreateOnUtc = DateTime.UtcNow,
                    HtmlContent = HttpUtility.HtmlEncode(request.HtmlContent),
                    CssContent = request.CssContent,
                    HideSidebar = request.HideSidebar
                };

                await _customPageRepository.AddAsync(customPage);
                return new SuccessResponse<Guid>(uid);
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Error {nameof(CreatePageAsync)}");
                return new FailedResponse<Guid>((int)ResponseFailureCode.GeneralException, e.Message);
            }
        }

        public async Task<Response> EditPageAsync(CreateEditCustomPageRequest request)
        {
            try
            {
                var page = await _customPageRepository.GetAsync(request.Id);
                if (null == page)
                {
                    throw new InvalidOperationException($"CustomPageEntity with Id '{request.Id}' is not found.");
                }

                page.Title = request.Title.Trim();
                page.RouteName = request.RouteName.ToLower().Trim();
                page.HtmlContent = HttpUtility.HtmlEncode(request.HtmlContent);
                page.CssContent = request.CssContent;
                page.HideSidebar = request.HideSidebar;
                page.UpdatedOnUtc = DateTime.UtcNow;

                await _customPageRepository.UpdateAsync(page);
                return new SuccessResponse();
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Error {nameof(EditPageAsync)}");
                return new FailedResponse<Guid>((int)ResponseFailureCode.GeneralException, e.Message);
            }
        }

        public Response DeletePage(Guid pageId)
        {
            try
            {
                var page = _customPageRepository.Get(pageId);
                if (null == page)
                {
                    throw new InvalidOperationException($"CustomPageEntity with Id '{pageId}' is not found.");
                }

                _customPageRepository.Delete(pageId);
                return new SuccessResponse();
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Error {nameof(DeletePage)}");
                return new FailedResponse<Guid>((int)ResponseFailureCode.GeneralException, e.Message);
            }
        }

        private CustomPage EntityToCustomPage(CustomPageEntity entity)
        {
            if (null == entity)
            {
                return null;
            }

            return new CustomPage
            {
                Id = entity.Id,
                Title = entity.Title.Trim(),
                CreateOnUtc = entity.CreateOnUtc,
                CssContent = entity.CssContent,
                RawHtmlContent = HttpUtility.HtmlDecode(entity.HtmlContent),
                HideSidebar = entity.HideSidebar,
                RouteName = entity.RouteName.Trim().ToLower(),
                UpdatedOnUtc = entity.UpdatedOnUtc
            };
        }
    }
}
