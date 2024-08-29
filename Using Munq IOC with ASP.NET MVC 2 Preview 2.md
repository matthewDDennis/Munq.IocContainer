<h2>Overview</h2>

<p>In this article, I will walk through the modification of the default ASP.NET MVC 2 application to use the Munq IOC container. This is a fairly simple process during which we will create a custom Controller Factory for the framework which you can use in other applications.</p>

<p>The previous article is available on my blog at <a href="http://rattlingaroundmybrain.spaces.live.com/blog/cns!99CC0775D5794768!211.entry">Introduction to Munq IOC Container for ASP.NET</a> or on The Code Project at <a href="http://www.codeproject.com/Articles/43296/Introduction-to-Munq-IOC-Container-for-ASP-NET.aspx">Introduction to Munq IOC Container for ASP.NET</a>. This article develops the base application from which we will examine the features of the Munq IOC in future articles.</p>

<h2>Step 1: Create an MVC 2 Project</h2>

<p>Open Visual Studio and create a new MVC 2 application. Give it any name. I called mine <code>FinalApp</code> to distinguish it from the <code>InitialApp</code> I created as a reference.</p>

<p>Build the application and familiarize yourself with the login/register/logout functionality of the <code>AccountController</code>. <strong>Hint</strong>: login is available in the upper right of the page.</p>

<h2>Step 2: Remove the Dependencies in AccountController</h2>

<p>The <code>AccountController</code> has two hard dependencies to concrete implementations to <code>FormsAuthenticationService</code> and <code>AccountMembershipService</code> as shown in the <strong>bold</strong> lines below:</p>

<pre lang="cs">
    // This constructor is used by the MVC framework to instantiate the controller using
    // the default forms authentication and membership providers.

    public AccountController()
        : this(null, null)
    {
    }

    // This constructor is not used by the MVC framework but is instead provided for ease
    // of unit testing this type. See the comments at the end of this file for more
    // information.
    public AccountController(IFormsAuthentication formsAuth, IMembershipService service)
    {
<strong>        FormsAuth = formsAuth ?? new FormsAuthenticationService();
        MembershipService = service ?? new AccountMembershipService();
</strong>    }</pre>

<p>To &lsquo;fix&rsquo; this, you need to:</p>

<ol>
	<li>Add a reference to the <em>Munq.DI.dll</em>.</li>
	<li>In the <em>global.asax</em>:
	<ol>
		<li>Create an application level Container variable</li>
		<li>Register the dependencies in the container on <code>Application_Start</code></li>
	</ol>
	</li>
	<li>Change the default constructor for <code>AccountController</code> to resolve the dependencies</li>
	<li>Change the constructor with parameters to check for <code>null</code> arguments</li>
</ol>

<p>Registering and resolving the Controllers will be handled later.</p>

<p>After steps 1 and 2, the <em>global.asax</em> should look like the listing below. Changes are highlighted.</p>

<pre lang="cs">
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
<strong>using Munq.DI;
using FinalApp.Controllers;</strong>

namespace FinalApp
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        <strong>public static Container IOC {get; private set;}</strong>
        
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute(&quot;{resource}.axd/{*pathInfo}&quot;);

            routes.MapRoute(
                &quot;Default&quot;,                                              // Route name
                &quot;{controller}/{action}/{id}&quot;,                           // URL with parameters
                 new { controller = &quot;Home&quot;, action = &quot;Index&quot;, id = &quot;&quot; }  // Parameter defaults
            );

        }

        protected void Application_Start()
        {
            <strong>IOC = new Container();
            RegisterInterfaces();</strong>
            RegisterRoutes(RouteTable.Routes);
        }

        <strong>private void RegisterInterfaces()
        {
            IOC.Register&lt;IFormsAuthentication&gt;( ioc =&gt; new AccountMembershipService()); </strong>
<strong>            IOC.Register&lt;IMembershipService&gt;( ioc =&gt; new FormsAuthenticationService());</strong>
<strong>     }
}</strong></pre>

<p>After steps 3 and 4, the <code>AccountController</code> constructors should look like:</p>

<pre lang="cs">
    [HandleError]
    public class AccountController : Controller
    {

        // This constructor is used by the MVC framework to instantiate the controller using
        // the default forms authentication and membership providers.
        public AccountController()
            : this(    MvcApplication.IOC.Resolve&lt;IFormsAuthentication&gt;(), 
              MvcApplication.IOC.Resolve&lt;IMembershipService&gt;())
        {
        }

        // This constructor is not used by the MVC framework but is instead provided for ease
        // of unit testing this type. See the comments at the end of this file for more
        // information.
        public AccountController(IFormsAuthentication formsAuth, IMembershipService service)
        {
            // Validate the parameters
            if (formsAuth == null)
                throw new ArgumentNullException(&quot;formsAuth&quot;);
    
            if (service == null)
                throw new ArgumentNullException(&quot;service&quot;);
            
            // set the dependencies    
            FormsAuth = formsAuth;
            MembershipService = service;
        }
        ...</pre>

<p>Now build and run the application. It should work as it did before we started.</p>

<p>Yes, it seems that we have done a lot of work to get the same functionality and traded two dependencies for a dependency on the IOC container. We will remove this dependency in the next step.</p>

<h2>Step 3: Create an IOC Aware ControllerFactory</h2>

<p>In order to remove the dependency the controllers have on the IOC container, it is necessary to create a custom <code>ControllerFactory</code>. This <code>ControllerFactory</code> will use the IOC container to create the correct controller and resolve any constructor dependencies.</p>

<p>Since we will want to reuse this in future projects, we will:</p>

<ol>
	<li>Create a new class library project</li>
	<li>Add references to <code>Munq.DI</code>, <code>System.Web.MVC</code>, and <code>System.Web.Routing</code></li>
	<li>Create the <code>MunqControllerFactory</code> class</li>
	<li>Register the new Controller Factory and register the controllers</li>
	<li>Remove the dependency on the Container from <code>AccountController</code></li>
	<li>Fix up the tests</li>
</ol>

<p>After steps 1-3, the project should look like:</p>

<p><strong><img alt="clip_image001[4]" src="clip_image0014_thumb1.png" style="width: 310px; height: 367px" title="clip_image001[4]" /> </strong></p>

<p>Because Munq can register factories by name, and Munq handles the caching of the factories and lookup performed by the <code>DefaultControllerFactory</code> class, we can derive a new <code>MunqControllerFactory</code> from the <code>IControllerFactory</code> <code>interface</code>. The one of the <code>CreateController</code> method&rsquo;s parameters is the name of the controller, without the <code>Controller</code> suffix. This means we can register the controllers by name. The other method that needs to be written is the <code>ReleaseController</code> method, which disposes of the controller if required. This is shown below. Notice that the constructor take a Munq Container as a parameter.</p>

<pre lang="cs">
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;

using Munq.DI;

namespace Munq.MVC
{
    public class MunqControllerFactory : IControllerFactory
    {
        public Container IOC { get; private set; }

        public MunqControllerFactory(Container container)
        {
            IOC = container;
        }

        #region IControllerFactory Members

        public IController CreateController(RequestContext requestContext, string controllerName)
        {
            try
            {
                return IOC.Resolve&lt;IController&gt;(controllerName);
            }
            catch
            {
                return null;
            }
        }

        public void ReleaseController(IController controller)
        {
            var disposable = controller as IDisposable;

            if (disposable != null)
            {
                disposable.Dispose();
            }
        }

        #endregion
    }
}</pre>

<p>The next step is to register the <code>MunqControllerFactory</code> as the default controller factory. Open up the <em>global.asax</em><code> </code>and modify it as shown:</p>

<pre lang="cs">
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Munq.DI;
using Munq.MVC;
using FinalApp.Controllers;

namespace FinalApp
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static Container IOC { get; private set; }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute(&quot;{resource}.axd/{*pathInfo}&quot;);

            routes.MapRoute(
                &quot;Default&quot;,                                              // Route name
                &quot;{controller}/{action}/{id}&quot;,                           // URL with parameters
                new { controller = &quot;Home&quot;, action = &quot;Index&quot;, id = &quot;&quot; }  // Parameter defaults
            );

        }

        protected void Application_Start()
        {
            InitializeIOC();
            RegisterRoutes(RouteTable.Routes);
        }

        private void InitializeIOC()
        {
            // Create the IOC container
            IOC = new Container();

            // Create the Default Factory
            var controllerFactory = new MunqControllerFactory(IOC);

            // set the controller factory
            ControllerBuilder.Current.SetControllerFactory(controllerFactory);

            // Register the dependencies
            IOC.Register&lt;&gt;IFormsAuthentication&gt;(ioc =&gt; new FormsAuthenticationService());
            IOC.Register&lt;IMembershipService&gt;(ioc =&gt; new AccountMembershipService());

            // Register the Controllers
            IOC.Register&lt;IController&gt;(&quot;Home&quot;, ioc =&gt; new HomeController());
            IOC.Register&lt;IController&gt;(&quot;Account&quot;,
                    ioc =&gt; new AccountController(ioc.Resolve&lt;IFormsAuthentication&gt;(),
                                                  ioc.Resolve&lt;IMembershipService&gt;())
            );
        }
    }
}</pre>

<p>Now we are ready to remove the dependency on the IOC from the <code>AccountController</code>. This is as simple as removing the default constructor as it references the IOC container. The <code>MunqControllerFactory</code> and the Munq IOC handle all the work of resolving the dependencies for the remaining constructor. The start of the <code>AccountController</code> should look like:</p>

<pre lang="cs">
    ...
    [HandleError]
    public class AccountController : Controller
    {
        public AccountController(IFormsAuthentication formsAuth, IMembershipService service)
        {
            // Validate the parameters
            if (formsAuth == null)
                throw new ArgumentNullException(&quot;formsAuth&quot;);

            if (service == null)
                throw new ArgumentNullException(&quot;service&quot;);

            // set the dependencies    
            FormsAuth = formsAuth;
            MembershipService = service;
        }
    ...</pre>

<p>The only thing left is to comment out the <code>AccountController</code> Unit Test which tests the constructor we just removed.</p>

<pre lang="cs">
...
        //[TestMethod]
        //public void ConstructorSetsPropertiesToDefaultValues()
        //{
        //    // Act
        //    AccountController controller = new AccountController();

        //    // Assert
        //    Assert.IsNotNull(controller.FormsAuth, &quot;FormsAuth property is null.&quot;);
        //    Assert.IsNotNull(controller.MembershipService, &quot;MembershipService property is null.&quot;);
        //}
...</pre>

<p>Now you can build, run the tests and the application. We now have a platform where we can now demonstrate the different Lifetime Managers and how they can simplify state management for your application. That however is something for the next article. In the next set of articles, I will build a real-world application, what I haven&rsquo;t decided so give me some ideas of what you would like to see.</p>

<p><strong>Note</strong>: This version of the <code>MunqControllerFactory</code> does not support the Areas feature of MVC 2. This too will be corrected in a future article.</p>

<p>del.icio.us Tags: <a href="http://del.icio.us/popular/ASP.NET" rel="tag">ASP.NET</a>,<a href="http://del.icio.us/popular/MVC" rel="tag">MVC</a>,<a href="http://del.icio.us/popular/IOC" rel="tag">IOC</a>,<a href="http://del.icio.us/popular/DI" rel="tag">DI</a></p>

<p><a href="http://anyurl.com/" rel="tag">CodeProject</a></p>
