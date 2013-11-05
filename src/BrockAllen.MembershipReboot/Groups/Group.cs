/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BrockAllen.MembershipReboot
{
    public class Group
    {
        public Group()
        {
            this.Children = new HashSet<GroupChild>();
        }

        protected internal virtual void Init(string tenant, string name)
        {
            if (String.IsNullOrWhiteSpace(tenant)) throw new ValidationException("Tenant is required.");
            if (String.IsNullOrWhiteSpace(name)) throw new ValidationException("Name is required.");

            this.Tenant = tenant;
            this.Name = name;
            this.Created = this.LastUpdated = UtcNow;
        }

        [Key]
        public virtual int Id { get; internal set; }

        [StringLength(50)]
        [Required]
        public virtual string Tenant { get; internal set; }
        
        [StringLength(100)]
        [Required]
        public virtual string Name { get; internal set; }

        public virtual DateTime Created { get; internal set; }
        public virtual DateTime LastUpdated { get; internal set; }

        public virtual ICollection<GroupChild> Children { get; internal set; }

        protected internal virtual DateTime UtcNow
        {
            get
            {
                return DateTime.UtcNow;
            }
        }

        internal void AddChild(int childGroupId)
        {
            if (this.Id == childGroupId) return;

            var child = this.Children.SingleOrDefault(x => x.ChildGroupId == childGroupId);
            if (child == null)
            {
                this.Children.Add(new GroupChild { GroupId = this.Id, ChildGroupId = childGroupId });
            }
        }
        
        internal void RemoveChild(int childGroupId)
        {
            if (this.Id == childGroupId) return;

            var child = this.Children.SingleOrDefault(x => x.ChildGroupId == childGroupId);
            if (child != null)
            {
                this.Children.Remove(child);
            }
        }
    }
}
