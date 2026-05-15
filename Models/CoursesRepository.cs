using DAL;
using System;
using System.Collections.Generic;

namespace Models
{
    public class CoursesRepository : Repository<Course>
    {
        public bool Update(Course data, List<int> selectedStudentsId)
        {
            try
            {
                BeginTransaction();
                if (base.Update(data))
                {
                    data.UpdateRegistrations(selectedStudentsId);
                    EndTransaction();
                    return true;
                }
                EndTransaction();
                return false;
            }
            catch
            {
                EndTransaction();
                return false;
            }
        }

        public override bool Delete(int courseId)
        {
            try
            {
                Course courseToDelete = DB.Courses.Get(courseId);
                if (courseToDelete != null)
                {
                    BeginTransaction();
                    courseToDelete.DeleteAllRegistrations();
                    courseToDelete.DeleteAllAllocations();
                    base.Delete(courseId);
                    EndTransaction();
                    return true;
                }
                return false;
            }
            catch
            {
                EndTransaction();
                return false;
            }
        }
    }
}
