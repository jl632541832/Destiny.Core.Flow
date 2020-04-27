﻿using Destiny.Core.Flow.Entity;
using Destiny.Core.Flow.Identity;
using Destiny.Core.Flow.Mapping;
using Destiny.Core.Flow.Model.Entities.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Destiny.Core.Flow.Dtos.RoleDtos
{
    [AutoMapp(typeof(Role))]
    public class RoleInputDto : InputDto<Guid>
    {
        public RoleInputDto() { }
        /// <summary>
        ///获取或设置 描述
        /// </summary>
        [DisplayName("描述")]
        public virtual string Description { get; set; }
        /// <summary>
        /// 角色名称
        /// </summary>
        [DisplayName("角色名称")]
        public string Name { get; set; }

        /// <summary>
        /// 标准化角色名称
        /// </summary>
        [DisplayName("标准化角色名称")]
        public string NormalizedName { get; set; }
        /// <summary>
        /// 是否管理员
        /// </summary>
        [DisplayName("是否管理员")]

        public bool IsAdmin { get; set; }
    }
}