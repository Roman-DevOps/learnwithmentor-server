﻿using System.Collections.Generic;
using System.Linq;
using LearnWithMentorDAL.Entities;
using LearnWithMentorDTO;

namespace LearnWithMentorDAL.Repositories
{
    public class UserRepository: BaseRepository<User>, IUserRepository
    {
        public UserRepository(LearnWithMentor_DBEntities context) : base(context)
        {
        }
        public User Get(int id)
        {
            return context.Users.FirstOrDefault(u => u.Id == id);
        }
        public void RemoveById(int id)
        {
            var item = context.Users.Where(u => u.Id == id);
            if (item.Any())
            {
                context.Users.RemoveRange(item);
            }
        }
        public void UpdateById(int id, UserDTO user)
        {
            var item = context.Users.Where(u => u.Id == id);
            if (item.Any())
            {
                User toUpdate = item.First();
                if (user.FirstName != null)
                {
                    toUpdate.FirstName = user.FirstName;
                }
                if (user.LastName != null)
                {
                    toUpdate.LastName = user.LastName;
                }
                var updatedRole = context.Roles.Where(r => r.Name == user.Role);
                if (updatedRole.Any())
                {
                    toUpdate.Role_Id = updatedRole.First().Id;
                }
                Update(toUpdate);
            }
        }
        public bool Add(UserDTO userDTO, string password)
        {
            if (userDTO.Email == null || userDTO.FirstName == null || userDTO.LastName == null || password == null)
            {
                return false;
            }
            else
            {
                User toAdd = new User();
                toAdd.Email = userDTO.Email;
                //add hashing
                toAdd.Password = password;
                toAdd.Role_Id = context.Roles.Where(r => r.Name == userDTO.Role).Any() ?
                    context.Roles.Where(r => r.Name == userDTO.Role).First().Id : context.Roles.Where(r => r.Name == "Student").First().Id;
                toAdd.FirstName = userDTO.FirstName;
                toAdd.LastName = userDTO.LastName;
                context.Users.Add(toAdd);
                return true;
            }
        }
        public IEnumerable<User> Search(string[] str, int? role_id)
        {
            List<User> ret = new List<User>();
            foreach (var s in str)
            {
                var found = role_id == null ? context.Users.Where(u => u.FirstName.Contains(s) || u.LastName.Contains(s)) :
                    context.Users.Where(u => u.Role_Id == role_id).Where(u => u.FirstName.Contains(s) || u.LastName.Contains(s));
                foreach (var f in found)
                {
                    if (!ret.Contains(f))
                    {
                        ret.Add(f);
                    }
                }
            }
            return ret;
        }
    }
}
