﻿using LearnWithMentorDAL.Entities;

namespace LearnWithMentorDAL.Repositories
{
    public interface IRoleRepository : IRepository<Role>
    {
        Role Get(int id);
        bool TryGetByName(string name, out Role role);
    }
}
