using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot
{
    public class GroupService
    {
        MembershipRebootConfiguration configuration;
        IGroupRepository groupRepository;

        public SecuritySettings SecuritySettings
        {
            get
            {
                return configuration.SecuritySettings;
            }
        }

        public GroupService(IGroupRepository groupRepository)
            : this(new MembershipRebootConfiguration(), groupRepository)
        {
        }

        public GroupService(MembershipRebootConfiguration configuration, IGroupRepository groupRepository)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            if (groupRepository == null) throw new ArgumentNullException("groupRepository");

            this.configuration = configuration;
            this.groupRepository = groupRepository;
        }

        public IQueryable<Group> GetAll()
        {
            return GetAll(null);
        }
        
        public IQueryable<Group> GetAll(string tenant)
        {
            if (!SecuritySettings.MultiTenant)
            {
                tenant = SecuritySettings.DefaultTenant;
            }

            if (String.IsNullOrWhiteSpace(tenant)) throw new ArgumentNullException("tenant");

            return this.groupRepository.GetAll().Where(x=>x.Tenant == tenant);
        }

        public Group Get(int groupId)
        {
            return this.groupRepository.Get(groupId);
        }

        public Group Create(string name)
        {
            return Create(null, name);
        }

        bool NameAlreadyExists(string tenant, string name, int? exclude = null)
        {
            var query = GetAll(tenant).Where(x => x.Name == name);
            if (exclude.HasValue)
            {
                query = query.Where(x => x.Id != exclude.Value);
            }
            return query.Any();
        }

        public Group Create(string tenant, string name)
        {
            if (!SecuritySettings.MultiTenant)
            {
                tenant = SecuritySettings.DefaultTenant;
            }

            if (NameAlreadyExists(tenant, name))
            {
                throw new ValidationException("That name is already in use.");
            }

            var grp = this.groupRepository.Create();
            grp.Init(tenant, name);

            this.groupRepository.Add(grp);

            return grp;
        }

        public void Delete(int groupId)
        {
            var grp = Get(groupId);
            if (grp == null) throw new ArgumentException("Invalid GroupID");

            this.groupRepository.Remove(grp);
            RemoveChildGroupFromOtherGroups(groupId);
        }

        private void RemoveChildGroupFromOtherGroups(int childGroupId)
        {
            var query =
                from g in this.groupRepository.GetAll()
                from c in g.Children
                where c.ChildGroupId == childGroupId
                select g;
            foreach (var group in query.ToArray())
            {
                group.RemoveChild(childGroupId);
                Update(group);
            }
        }

        private void Update(Group group)
        {
            group.LastUpdated = group.UtcNow;
            this.groupRepository.Update(group);
        }

        public void ChangeName(int groupId, string name)
        {
            if (String.IsNullOrWhiteSpace(name)) throw new ValidationException("Invalid name.");

            var group = Get(groupId);
            if (group == null) throw new ArgumentException("Invalid GroupID");
            
            if (NameAlreadyExists(group.Tenant, name, groupId))
            {
                throw new ValidationException("That name is already in use.");
            }

            group.Name = name;
            Update(group);
        }

        public void AddChildGroup(int groupId, int childGroupId)
        {
            var group = Get(groupId);
            if (group == null) throw new ArgumentException("Invalid GroupID");

            var childGroup = Get(childGroupId);
            if (childGroup == null) throw new ArgumentException("Invalid ChildGroupID");

            group.AddChild(childGroupId);
            Update(group);
        }
        
        public void RemoveChildGroup(int groupId, int childGroupId)
        {
            var group = Get(groupId);
            if (group == null) throw new ArgumentException("Invalid GroupID");

            group.RemoveChild(childGroupId);
            Update(group);
        }
    }
}
