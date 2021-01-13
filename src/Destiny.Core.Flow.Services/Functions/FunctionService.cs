﻿using Destiny.Core.Flow.Dtos.Functions;
using Destiny.Core.Flow.Enums;
using Destiny.Core.Flow.Exceptions;
using Destiny.Core.Flow.Extensions;
using Destiny.Core.Flow.Filter;
using Destiny.Core.Flow.Filter.Abstract;
using Destiny.Core.Flow.IServices.Functions;
using Destiny.Core.Flow.Model.Entities.Function;
using Destiny.Core.Flow.Repository.MenuRepository;
using Destiny.Core.Flow.Ui;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Destiny.Core.Flow.Services.Functions
{
    public class FunctionService : IFunctionService
    {
        private readonly IRepository<Function, Guid> _functionRepository;
        private readonly IMenuFunctionRepository _menuFunctionRepository = null;

        public FunctionService(IRepository<Function, Guid> functionRepository, IMenuFunctionRepository menuFunctionRepository)
        {
            _functionRepository = functionRepository ?? throw new ArgumentNullException(nameof(functionRepository));
            _menuFunctionRepository = menuFunctionRepository;
        }

        public async Task<OperationResponse> CreateAsync(FunctionInputDto dto)
        {
            dto.NotNull(nameof(dto));

            return await _functionRepository.InsertAsync(dto, async f =>
            {
                bool isExist = await this.Entities.Where(o => o.LinkUrl.ToLower() == dto.LinkUrl.ToLower()).AnyAsync();
                if (isExist)
                {
                    throw new AppException("此功能已存在!!!");
                }
            });
        }

        private IQueryable<Function> Entities => _functionRepository.Entities;

        public Task<OperationResponse> DeleteAsync(Guid id)
        {
            id.NotEmpty(nameof(id));
            return _functionRepository.DeleteAsync(id);
        }

        public async Task<IPagedResult<FunctionOutputPageList>> GetFunctionPageAsync(FunctionPageRequestDto request)
        {

            OrderCondition<Function>[] orderConditions = new OrderCondition<Function>[] { new OrderCondition<Function>(o => o.CreatedTime, SortDirection.Descending) };
            request.OrderConditions = orderConditions;
            Expression<Func<Function, bool>> expression = o => true;
            if (request.MenuIds?.Any()==true)
            {
                var functionIds=  _menuFunctionRepository.Entities.Where(o => request.MenuIds.Contains(o.MenuId)).Select(o => o.FunctionId);
                expression = o => !functionIds.Contains(o.Id);
                //if (functionIds.Any())
                //{
                //    request.Filter.Conditions.Add(new FilterCondition() { Field = "Id", Value = functionIds, Operator = FilterOperator.In });
                //}
            }
            return await _functionRepository.Entities.Where(expression).ToPageAsync<Function, FunctionOutputPageList>(request);
        }

        public async Task<OperationResponse<FunctionOutputDto>> LoadFormFunctionAsync(Guid id)
        {
            id.NotEmpty(nameof(id));
            var functionDto = await _functionRepository.GetByIdToDtoAsync<FunctionOutputDto>(id);
            return new OperationResponse<FunctionOutputDto>("加载成功", functionDto, OperationResponseType.Success);
        }

        public async Task<OperationResponse> UpdateAsync(FunctionInputDto dto)
        {
            dto.NotNull(nameof(dto));
            return await _functionRepository.UpdateAsync(dto, async (f, e) =>
            {
                bool isExist = await this.Entities.Where(o => o.Id != f.Id && o.LinkUrl.ToLower() == f.LinkUrl.ToLower()).AnyAsync();
                if (isExist)
                {
                    throw new AppException("此功能已存在!!!");
                }
            });
        }

        public async Task<OperationResponse<IEnumerable<SelectListItem>>> GetFunctionSelectListItemAsync()
        {
            var functions = await _functionRepository.Entities.OrderBy(o => o.Name).Select(x => new SelectListItem
            {
                Value = x.Id.ToString().ToLower(),
                Text = x.Name,
                Selected = false
            }).ToListAsync();
            return new OperationResponse<IEnumerable<SelectListItem>>(MessageDefinitionType.DataSuccess, functions, OperationResponseType.Success);
        }
    }
}