using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;

namespace EntityFramework.Testing.FakeItEasy {
    public class FakeEntityFramework {

        public static T FakeDbContext<T>() where T : class {
            var fakeDbContext = A.Fake<T>();
            return fakeDbContext;
        }


        public static DbSet<T> FakeDbSet<T>(IList<T> data) where T : class {


            var fakeDbSet = A.Fake<DbSet<T>>(b => b.Implements(typeof(IQueryable<T>)));

            QueryableSetUp(fakeDbSet, data, false);
            SetUpFakeDbSetBehaviour(fakeDbSet, data);

            return fakeDbSet;
        }


        public static DbSet<T> FakeDbSetAsync<T>(IList<T> data) where T : class {

            var fakeDbSet = A.Fake<DbSet<T>>(s => s.Implements(typeof(IQueryable<T>))
                .Implements(typeof(IDbAsyncEnumerable<T>)));

            QueryableSetUp(fakeDbSet, data);
            SetUpFakeDbSetBehaviour(fakeDbSet, data);

            return fakeDbSet;
        }


        private static void QueryableSetUp<T>(IQueryable<T> fakeDbSet, IEnumerable<T> data, bool isAsync = true)
            where T : class {

            var asQueryable = data.AsQueryable();

            if (isAsync) {

                A.CallTo(() => ((IDbAsyncEnumerable<T>)fakeDbSet).GetAsyncEnumerator())
                    .ReturnsLazily(info => new TestDbAsyncEnumerator<T>(asQueryable.GetEnumerator()));
                A.CallTo(() => fakeDbSet.Provider)
                    .ReturnsLazily(info => new TestDbAsyncQueryProvider<T>(asQueryable.Provider));

            } else {
                A.CallTo(() => fakeDbSet.Provider).ReturnsLazily(l => asQueryable.Provider);
            }

            A.CallTo(() => fakeDbSet.Expression).ReturnsLazily(l => asQueryable.Expression);
            A.CallTo(() => fakeDbSet.ElementType).ReturnsLazily(l => asQueryable.ElementType);
            A.CallTo(() => fakeDbSet.GetEnumerator()).ReturnsLazily(l => asQueryable.GetEnumerator());
        }



        private static void SetUpFakeDbSetBehaviour<T>(DbSet<T> fakeDbSet, ICollection<T> data) where T : class {

            SetupAsNoTracking(fakeDbSet);

            SetupIncludeForFakeDbSet(fakeDbSet);

            SetUpFindForFakeDbSet(fakeDbSet);

            SetupFindAsyncForFakeDbSet(fakeDbSet);

            SetupFindAsyncWithCancellationForFakeDbSet(fakeDbSet);

            SetupCreateForFakeDbSet(fakeDbSet);

            SetupRemoveForFakeDbSet(fakeDbSet, data);

            SetupRemoveRangeForFakeDbSet(fakeDbSet, data);

            SetupAddForFakeDbSet(fakeDbSet, data);

            SetupAddRangeForFakeDbSet(fakeDbSet, data);

            SetupAttachForFakeDbSet(fakeDbSet, data);
        }


        private static void SetupAttachForFakeDbSet<T>(DbSet<T> fakeDbSet, ICollection<T> data) where T : class {
            A.CallTo(() => fakeDbSet.Attach(A<T>._)).ReturnsLazily<T, T>(entity => {
                data.Add(entity);
                return entity;
            });
        }

        private static void SetupAddRangeForFakeDbSet<T>(DbSet<T> fakeDbSet, ICollection<T> data) where T : class {
            A.CallTo(() => fakeDbSet.AddRange(A<IEnumerable<T>>._))
                .ReturnsLazily<IEnumerable<T>, IEnumerable<T>>(entities => {
                    foreach (var entity in entities) {
                        data.Add(entity);
                    }

                    return entities;
                });
        }

        private static void SetupAddForFakeDbSet<T>(DbSet<T> fakeDbSet, ICollection<T> data) where T : class {
            A.CallTo(() => fakeDbSet.Add(A<T>._)).ReturnsLazily<T, T>(entity => {
                data.Add(entity);
                return entity;
            });
        }

        private static void SetupRemoveRangeForFakeDbSet<T>(DbSet<T> fakeDbSet, ICollection<T> data) where T : class {
            A.CallTo(() => fakeDbSet.RemoveRange(A<IEnumerable<T>>._))
                .ReturnsLazily<IEnumerable<T>, IEnumerable<T>>(entities => {
                    foreach (var entity in entities) {
                        data.Remove(entity);
                    }

                    return entities;
                });
        }

        private static void SetupRemoveForFakeDbSet<T>(DbSet<T> fakeDbSet, ICollection<T> data) where T : class {
            A.CallTo(() => fakeDbSet.Remove(A<T>._)).ReturnsLazily<T, T>(entity => {
                data.Remove(entity);
                return entity;
            });
        }


        private static void SetupCreateForFakeDbSet<T>(DbSet<T> fakeDbSet) where T : class {

            A.CallTo(() => fakeDbSet.Create()).ReturnsLazily(() => Activator.CreateInstance<T>());
        }


        private static void SetupFindAsyncWithCancellationForFakeDbSet<T>(DbSet<T> fakeDbSet) where T : class {

            Func<object[], T> find = (o => null);

            A.CallTo(() => fakeDbSet.FindAsync(A<CancellationToken>._, A<object[]>._))
                .ReturnsLazily<Task<T>, CancellationToken, object[]>((token, objs) => Task.Run(() => find(objs), token));
        }


        private static void SetupFindAsyncForFakeDbSet<T>(DbSet<T> fakeDbSet) where T : class {

            Func<object[], T> find = (o => null);

            A.CallTo(() => fakeDbSet.FindAsync(A<object[]>._))
                .ReturnsLazily<Task<T>, object[]>(objs => Task.Run(() => find(objs)));
        }


        private static void SetUpFindForFakeDbSet<T>(DbSet<T> fakeDbSet) where T : class {

            Func<object[], T> find = (o => null);

            A.CallTo(() => fakeDbSet.Find(A<object[]>._)).ReturnsLazily<T, object[]>(objs => find(objs));
        }


        private static void SetupIncludeForFakeDbSet<T>(DbSet<T> fakeDbSet) where T : class {
            A.CallTo(() => fakeDbSet.Include(A<string>._)).Returns(fakeDbSet);
        }


        private static void SetupAsNoTracking<T>(DbSet<T> fakeDbSet) where T : class {

            A.CallTo(() => fakeDbSet.AsNoTracking()).Returns(fakeDbSet);
        }
    }
}