<h2>Table of Contents</h2>

<ul>
	<li><a href="#Mail">&ldquo;You have mail&rdquo;</a></li>
	<li><a href="#LifeTimeManager">What is a Lifetime Manager?</a></li>
	<li><a href="#Available">What Lifetime Managers are available?</a></li>
	<li><a href="#Examining">Examining an Existing Lifetime Manager</a></li>
	<li><a href="#Creating">Creating a new Lifetime Manager</a></li>
	<li><a href="#Conclusion">Conclusion</a></li>
</ul>

<h2><a name="Mail">&ldquo;You Have Mail&rdquo;</a></h2>

<p>Just as I was starting on my second article about Using Munq IocContainer V2 in ASP.NET MVC2 Applications, I received the following email.</p>

<p><em><strong>Miro Bartanus</strong> has posted a new comment at &quot;</em><a href="http://www.codeproject.com/Messages/3481648/Thread-Local-Storage-as-Lifetime.aspx"><em>[Article]: Introduction to Munq IOC Container for ASP.NET</em></a><em>&quot;: </em></p>

<p><em>Hi Matthew, Munq is very nice, I am just wondering if it supports TLS as a Lifetime, I will check sources later today, but you maid (might) know, or that should not be too difficult to implement...</em></p>

<p>and I thought, &ldquo;What a great idea.&rdquo; I&rsquo;d already planned an article on creating a Lifetime Manager, but wasn&rsquo;t sure of the type. A <code>ThreadLocalStorageLifetimeManager</code> is relatively easy, and it&#39;s something I need to solve some of my own programming issues now that I&rsquo;ve started to use the Parallel Programming library in .NET 4.</p>

<h2><a name="LifeTimeManager">What is a Lifetime Manager?</a></h2>

<p>A <code>LifetimeManager</code> controls the reuse of instances when the IOC Container is asked to Resolve a type. The <code>LifetimeManager</code> used when resolving can be specified as a default on the <code>Container</code> by calling the <code>UsesDefaultLifetimeManagerOf</code> method. The following example sets the default <code>LifetimeManager</code> to the <code>RequestLifetimeManger,</code> causing instances to be reused for the duration of each HTTP Request.</p>

<pre lang="cs">
// create the container.  Only done once in Application_Start
IIocContainer iocContainer = new Container();

// create a lifetime manager to use as default
ILifetimeManager lifetimeManager = new LifetimeManagers.RequestLifetime();

// set the default lifetime manager
iocContainer.UsesDefaultLifetimeManagerOf(lifetimeManager);</pre>

<p>Alternately, you can call the <code>WithLifetimeManager</code> method on the <code>IRegistration</code> instance returned from the <code>RegisterXXX</code> call. The following example registers two services and causes the same instance to always be returned from the container, effectively making them singletons.</p>

<pre lang="cs">
// create the container.  Only done once in Application_Start
IIocContainer iocContainer = new Container();

// create a Container lifetime manager to use for &#39;singleton&#39; services
// only one instance will be created and reused for each resolve request.
ILifetimeManager containerLifetimeManager = new LifetimeManagers.ContainerLifetime();

iocContainer.Register&lt;IMembershipService&gt;( ioc =&gt; 
	new AccountMembershipService(Membership.Provider))
            .WithLifetimeManager(containerLifetimeManager);

iocContainer.Register&lt;IFormsAuthenticationService&gt;
	(ioc =&gt; new FormsAuthenticationService())
            .WithLifetimeManager(containerLifetimeManager);</pre>

<h2><a name="Available">What Lifetime Managers are Available?</a></h2>

<p>Munq has a number of <code>LifetimeManager</code>s included with the version 2.0 release. These are described below. I will be adding the <code>ThreadLocalStorageLifetimeManger</code> to a future point release.</p>

<p><strong>Warning</strong>: If you used the <code>RegisterInstance</code> method, then the same instance will be returned regardless of which lifetime manager is used.</p>

<dl>
	<dt>AlwaysNewLifetime</dt>
	<dd>This lifetime manager&rsquo;s behaviour is to always return a new instance when the <code>Resolve</code> method is called by executing the factory method. This is the default behaviour.</dd>
	<dt>ContainerLifetime</dt>
	<dd>This lifetime manager&rsquo;s behaviour is to always return the same instance when the <code>Resolve</code> method is called by executing the factory method. The instance is cached in the container itself.</dd>
	<dt>SessionLifetime</dt>
	<dd>This lifetime manager&rsquo;s behaviour is to always return an attempt to retrieve the instance from <code>Session</code> when the <code>Resolve</code> method is called. If the instance does not exist in <code>Session</code>, then a new instance is created by executing the factory method, and storing it in the <code>Session</code>.</dd>
	<dt>RequestLifetime</dt>
	<dd>This lifetime manager&rsquo;s behaviour is to always return an attempt to retrieve the instance from <code>Request.Items </code>when the <code>Resolve </code>method is called. If the instance does not exist in <code>Request.Items</code>, then a new instance is created by executing the factory method, and storing it in the <code>Request.Items</code>.</dd>
	<dt>CachedLifetime</dt>
	<dd>This lifetime manager&rsquo;s behaviour is to always return an attempt to retrieve the instance from <code>Cache</code> when the <code>Resolve</code> method is called. If the instance does not exist in <code>Cache</code>, then a new instance is created by executing the factory method, and storing it in the <code>Cache</code>. CacheDependencies and Sliding or Absolute Timeouts can be applied to the <code>CachedLifetimeManager</code>.</dd>
</dl>

<h2><a name="Examining">Examining an Existing Lifetime Manager</a></h2>

<p>A Lifetime Manager implements the <code>ILifetimeManager</code> interface and its two methods. The first method, <code>GetInstance</code>, gets the requested instance from the <code>LifetimeManager</code>&rsquo;s cache, or creates an new instance if there is no cached instance. The second method, <code>InvalidateInstanceCache</code>, removes any previously created and cached instance, forcing a new instance to be created on the next resolve request.</p>

<pre lang="cs">
public interface ILifetimeManager
{
    object GetInstance(IInstanceCreator creator);
    void InvalidateInstanceCache(IRegistration registration);
}</pre>

<p>Below is the code for the <code>RequestLifetimeManager</code>. In the <code>GetInstance</code> method, the code attempts to retrieve an instance from the <code>HttpContext.Request.Items</code> collection using the <code>Key</code> property of the <code>IInstanceCreator</code>, creator, passed in. If the instance does not exist, the <code>IInstanceCreator CreateInstance</code> method is called, specifying that the instance is not to be cached in the container. This returns a new instance of the required type, and it is stored in the <code>HttpContext.Request.Items</code> collection for future reuse.</p>

<p>The <code>InvalidateInstanceCache</code> method just removes any stored instance, forcing the creation of a new instance on the next resolve request.</p>

<p>The other code is to support testing and can be ignored.</p>

<pre lang="cs">
using System.Web;

namespace Munq.LifetimeManagers
{
    public class RequestLifetime : ILifetimeManager
    {
        private HttpContextBase testContext;

        /// &lt;summary&gt;
        /// Return the HttpContext if running in a web application, the test 
        /// context otherwise.
        /// &lt;/summary&gt;
        private HttpContextBase Context
        {
            get
            {
                HttpContextBase context = (HttpContext.Current != null)
                                ? new HttpContextWrapper(HttpContext.Current)
                                : testContext;
                return context;
            }
        }

       #region ILifetimeManage Members
        /// &lt;summary&gt;
        /// Gets the instance from the Request Items, if available, 
        /// otherwise creates a new
        /// instance and stores in the Request Items.
        /// &lt;/summary&gt;
        /// &lt;param name=&quot;creator&quot;&gt;The creator (registration) to create a new instance.
        /// &lt;/param&gt;
        /// &lt;returns&gt;The instance.&lt;/returns&gt;
        public object GetInstance(IInstanceCreator creator)
        {
            object instance = Context.Items[creator.Key];
            if (instance == null)
            {
                instance = creator.CreateInstance
		(ContainerCaching.InstanceNotCachedInContainer);
                Context.Items[creator.Key] = instance;
            }

            return instance;
        }
        /// &lt;summary&gt;
        /// Invalidates the cached value.
        /// &lt;/summary&gt;
        /// &lt;param name=&quot;registration&quot;&gt;The Registration 
        /// which is having its value invalidated&lt;/param&gt;
        public void InvalidateInstanceCache(IRegistration registration)
        {
            Context.Items.Remove(registration.Key);
        }

        #endregion

        // only used for testing.  Has no effect when in web application
        public void SetContext(HttpContextBase context)
        {
            testContext = context;
        }
    }
}</pre>

<h2><a name="Creating">Creating a new Lifetime Manager</a></h2>

<p>Now you have enough knowledge to create a new <code>LifetimeManager</code>. The basic steps are:</p>

<ol>
	<li>Create a class derived from <code>ILifetimeManger</code></li>
	<li>Implement the two methods</li>
</ol>

<p>I am going to create these in the source for <code>Munq.IocContainer</code> because I think this a great addition, but you could create a new project and include a reference to <code>Munq.Interfaces</code>. Then if you needed your custom <code>LifetimeManager</code>, you would reference your DLL.</p>

<p>The class needs a thread local dictionary or hashtable to store the instances. Otherwise, the code is just about the same as the code for <code>RequestLifetimeManager</code>. The full code is:</p>

<pre lang="cs">
&gt;using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Munq.LifetimeManagers
{
    /// &lt;summary&gt;
    /// A LifetimeManager that uses Thread Local Storage to cache instances.
    /// &lt;/summary&gt;
    public class ThreadLocalStorageLifetime : ILifetimeManager
    {
        // The thread local storage.  The ThreadStatic attribute makes this easy.
        [ThreadStatic]
        static Dictionary&lt;string, object&gt; localStorage;

        /// &lt;summary&gt;
        /// Gets an instance from the thread local storage, 
        /// or creates a new instance if not found.
        /// &lt;/summary&gt;
        /// &lt;param name=&quot;creator&quot;&gt;The IInstanceCreate to use to get the Key 
        /// and create new if required.&lt;/param&gt;
        /// &lt;returns&gt;The instance.&lt;/returns&gt;
        public object GetInstance(IInstanceCreator creator)
        {
            object instance = null;

            // if it is a new thread then the localStorage needs to be initialized;
            if (localStorage == null)
                localStorage = new Dictionary&lt;string,object&gt;();
 
            if (!localStorage.TryGetValue(creator.Key, out instance))
            {
                instance = creator.CreateInstance
			(ContainerCaching.InstanceNotCachedInContainer);
                localStorage[creator.Key] = instance;
            }

            return instance;
        }

        /// &lt;summary&gt;
        /// Removes the instance for the registration from the local storage cache.
        /// &lt;/summary&gt;
        /// &lt;param name=&quot;registration&quot;&gt;The IRegistration returned 
        /// when the type was registered in the IOC container.&lt;/param&gt;
        public void InvalidateInstanceCache(IRegistration registration)
        {
            // nothing stored yet
            if (localStorage == null)
                return;

            localStorage.Remove(registration.Key);
        }
    }
}</pre>

<p>Unit tests for this are:</p>

<pre lang="cs">
&gt;using Munq.LifetimeManagers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Munq;
using System.Web;
using MvcFakes;
using Munq.FluentTest;
using System.Threading.Tasks;

namespace Munq.Test
{      
    /// &lt;summary&gt;
    ///This is a test class for ThreadLocalStorageLifetimeTest and is intended
    ///to contain all ThreadLocalLifetimeTest Unit Tests
    ///&lt;/summary&gt;
    [TestClass()]
    public class ThreadLocalStorageLifetimeTest
    {
        private TestContext testContextInstance;

        /// &lt;summary&gt;
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///&lt;/summary&gt;
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}

        IIocContainer iocContainer;
        // Use TestInitialize to run code before running each test 
        [TestInitialize()]
        public void MyTestInitialize()
        {
            iocContainer = new Munq.Container();
        }

        // Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
        {
            // remove the registrations, and cache values

            var regs = iocContainer.GetRegistrations&lt;IFoo&gt;();
            regs.ForEach(reg =&gt; iocContainer.Remove(reg));

            iocContainer.Dispose();
        }
        #endregion

        /// &lt;summary&gt;
        /// Verify that Can Set the DefaultLifetimeManager To ThreadLocalStorageLifetime
        ///&lt;/summary&gt;
        [TestMethod()]
        public void CanSetDefaultLifetimeManagerToThreadLocalStorageLifetime()
        {
            var lifetime = new ThreadLocalStorageLifetime();
            iocContainer.UsesDefaultLifetimeManagerOf(lifetime);

            Verify.That(iocContainer.LifeTimeManager).IsTheSameObjectAs(lifetime);
        }

        /// &lt;summary&gt;
        /// verify Request Lifetime returns same instance for same request, 
        /// different for different request
        /// &lt;/summary&gt;
        [TestMethod]
        public void ThreadLocalStorageLifetimeManagerReturnsSameObjectForSameRequest()
        {

            var requestltm = new ThreadLocalStorageLifetime();

            var container = new Container();
            container.Register&lt;IFoo&gt;(c =&gt; new Foo1())
                .WithLifetimeManager(requestltm);

            IFoo result1 = container.Resolve&lt;IFoo&gt;();
            IFoo result2 = container.Resolve&lt;IFoo&gt;();

            IFoo result3=null;
            IFoo result4=null;

            // get values on a different thread
            var t = Task.Factory.StartNew(() =&gt;
            {
                result3 = container.Resolve&lt;IFoo&gt;();
                result4 = container.Resolve&lt;IFoo&gt;();
            });

            t.Wait();

            // check the results
            Verify.That(result3).IsNotNull(); 
            Verify.That(result4).IsNotNull()
                        .IsTheSameObjectAs(result3);

            Verify.That(result2).IsNotNull();
            Verify.That(result1).IsNotNull()
                        .IsTheSameObjectAs(result2)
                        .IsNotTheSameObjectAs(result3);
        }
    }
}</pre>

<h2><a name="Conclusion">Conclusion</a></h2>

<p>Adding your own custom Lifetime Manager is simple, and allows you to support any custom data storage, caching, sessions, etc. that you may have written or use. How about an <code>AppFabricLifetimeManager</code>?</p>

<p>Please watch for future articles.</p>
