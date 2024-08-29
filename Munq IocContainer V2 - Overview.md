<h3>Table of Contents</h3>

<ul>
	<li>Downloading the code</li>
	<li>Overview</li>
	<li>What is Munq IocContainer</li>
	<li>Using Munq IocContainer</li>
	<li>Registering Factory Methods</li>
	<li>Obtaining an Instance from the IocContainer</li>
	<li>Initializing the IocContainer</li>
	<li>Lifetime Management</li>
</ul>

<h2>Downloading the Code</h2>

<p>The code is maintained at <a href="http://munq.codeplex.com/" target="_blank">CodePlex</a>.&nbsp; The latest release can be found <a href="http://munq.codeplex.com/releases/view/45775" target="_blank">here</a>.</p>

<h2>Overview</h2>

<p>This post is the first of several heralding the release of the first beta of the second release <strong>Munq</strong> <strong>IocContainer</strong>. In previous posts, <a href="http://rattlingaroundmybrain.spaces.live.com/Blog/cns!99CC0775D5794768!211.entry" target="_blank">Introduction to Munq IOC Container for ASP.NET</a> and <a href="http://rattlingaroundmybrain.spaces.live.com/Blog/cns!99CC0775D5794768!215.entry" target="_blank">Using Munq IOC with ASP.NET MVC 2 Preview 2</a>, I introduced an IOC Container that is high performance and designed for ASP.NET.&nbsp; This latest release fixes a number of small issues and adds functionality without sacrificing any of the performance.&nbsp; Unit tests were created to ensure the quality and correctness of the implement.&nbsp; I felt this was critical as several people have enquired about using <strong>Munq IocContainer</strong> in production code.</p>

<p>In addition, I will be releasing <strong>Munq FluentTest</strong>, a fluent interface to be used with MSTest or NUnit to allow the verification statements in a test to be written in an almost English syntax.&nbsp; Examples can be seen in the unit tests for the IocContainer.</p>

<p>The first articles in this series will be:</p>

<ol>
	<li>Munq IocContainer V2 &ndash; Overview (this article)</li>
	<li>Munq IocContainer V2 &ndash; Using Munq in an ASP.NET application (may be in two parts)</li>
	<li>Munq IocContainer V2 &ndash; Using Lifetime Management to Eliminate Unnecessary Instance Creation</li>
	<li>Munq IocContainer V2 &ndash; Using Munq to Implement an Automatic Plug-in or Modular Application</li>
	<li>Munq IocContainer V2 &ndash; Creating a custom Lifetime Manager.</li>
</ol>

<p>The previous list will be updated as changes and wild ideas occur.</p>

<h2>What is Munq IocContainer</h2>

<p>If you have been, or trying to, program using the <strong>SOLID</strong> principles then you are implementing your classes with <em>Dependency Inversion, </em>the <strong>D</strong> in SOLID.&nbsp; Simply stated, <em>Dependency Inversion</em> moves the responsibility of resolving dependencies to the caller of a method or constructor, instead the method or constructor.&nbsp; This makes the called method easier to test as the dependencies can be substituted with a mock or stub by unit test code.&nbsp; At its simplest, you can use the Poor Man&rsquo;s Dependency Injection pattern.&nbsp; This involve using two constructors, one which has the dependencies passed in, and a default (parameter-less constructor) that calls the other constructor with default dependency implementation.&nbsp; This is shown in the code snippet below:</p>

<div>
<div>
<pre style="BORDER-BOTTOM-STYLE: none; TEXT-ALIGN: ; PADDING-BOTTOM: 0px; LINE-HEIGHT: 12pt; BACKGROUND-COLOR: #f4f4f4; MARGIN: 0em; BORDER-LEFT-STYLE: none; PADDING-LEFT: 0px; WIDTH: 100%; PADDING-RIGHT: 0px; FONT-FAMILY: 'Courier New', courier, monospace; DIRECTION: ltr; BORDER-TOP-STYLE: none; COLOR: black; BORDER-RIGHT-STYLE: none; FONT-SIZE: 10pt; OVERFLOW: visible; PADDING-TOP: 0px">
public class MyClass
{
    // fields to hold references to the concrete implementations of
    // the class&#39;s dependencies.
    IDependencyOne _dependencyOne;
    IDependencyTwo _depencencyTwo;

    // constructor which allows the dependencies to be passed in.
    public MyClass(IDepenencyOne dependOne, IDependencyTwo dependTwo)
    {
        _dependencyOne = dependOne;
        _dependencyOne = dependTwo;
    }

    // default constructor which uses default implementations of
    // the class&#39;s dependencies/
    public MyClass() : this(new ImplDependOne(), new ImplDependTwo())
    {
    }

    public void AMethod(int a)
    {
        var b = _dependencyOne.MethodA(a);
        _dependencyTwo.MethodB(b);
    }
    ...
}        
</pre>
</div>
</div>

<p>Production code calls the default constructor, and unit tests use the constructor with parameters. Unfortunately, this still couples <em>MyClass</em> to the concrete implementations <em>ImplDependOne</em> and <em>ImplDependTwo</em>.</p>

<p>This dependency can be removed by removing the default constructor.&nbsp; However, this now means that every place that an instance of <em>MyClass</em> is created, the code must also create instances of classes implementing <em>IDependencyOne</em> and <em>IDependencyTwo</em>.&nbsp; This is where an IOC Container comes in.&nbsp; IOC Containers can be queried for an implementation of an interface or abstract class, and will return an instance with all of its dependencies resolved.</p>

<p>With <strong>Munq IocContainer</strong>, this requires two steps.&nbsp; The first is the registration of a factory method with the container, and second, using the container to resolve the dependency.&nbsp; <strong>Munq IocContainer</strong> registers methods that take an instance of <strong>IocContainer</strong> and returns an instance that implements the requested interface.&nbsp; By using factory methods instead of classes, the developer has complete control over what is done to create the instance.&nbsp; Furthermore, because there is no need to use <em>Reflection</em> or other CPU expensive techniques to select a constructor and resolve its dependencies, the resolution speed of the container is as fast as possible.&nbsp; Thus the previous example would look like:</p>

<pre style="BORDER-BOTTOM-STYLE: none; TEXT-ALIGN: ; PADDING-BOTTOM: 0px; LINE-HEIGHT: 12pt; BACKGROUND-COLOR: #f4f4f4; MARGIN: 0em; BORDER-LEFT-STYLE: none; PADDING-LEFT: 0px; WIDTH: 100.1%; PADDING-RIGHT: 0px; FONT-FAMILY: 'Courier New', courier, monospace; DIRECTION: ltr; BORDER-TOP-STYLE: none; HEIGHT: 707px; COLOR: black; BORDER-RIGHT-STYLE: none; FONT-SIZE: 10pt; OVERFLOW: visible; PADDING-TOP: 0px">
public class MyClass : IMyClass
{
    // fields to hold references to the concrete implementations of
    // the class&#39;s dependencies.
    IDependencyOne _dependencyOne;
    IDependencyTwo _depencencyTwo;



    // constructor which allows the dependencies to be passed in.
    public MyClass(IDepenencyOne dependOne, IDependencyTwo dependTwo)
    {
        _dependencyOne = dependOne;
        _dependencyOne = dependTwo;
    }



    public void AMethod(int a)
    {
        var b = _dependencyOne.MethodA(a);
        _dependencyTwo.MethodB(b);
    }
    ...
}       
... 
/////////////////////////////////////////////////////////////////////////////
// Registration, probably in Global.asax for an ASP.NET application
public static Container { get; private set;}



private void InitIocContainer()
{
    // create the container
    Container = new IocContainer();



    // Register the interface implementations.
    Container.Register&lt;IMyClass&gt;(c =&gt; new MyClass(c.Resolve&lt;IDependencyOne&gt;(), c.Resolve&lt;IDependencyTwo&gt;()) );
    Container.Register&lt;IDependencyOne&gt;(c =&gt; new ImplDependOne() );
    Container.Register&lt;IDependencyTwo&gt;(c =&gt; new ImplDependTwo() );
}

... 
/////////////////////////////////////////////////////////////////////////////
<strong>    // getting the class instance from the container
    IMyClass instance = MyApp.Container.Resolve&lt;IMyClass&gt;();
    instance.MethodA(42);</strong>
</pre>

<p>If you are familiar with any of the more well known IOC Containers such as Unity, NInject, AutoFac, Windsor, or StructureMap, you will be thinking, &ldquo;This is just the same as all the others&rdquo;.&nbsp; And you would be mainly correct.&nbsp; The difference is in the speed of the Resolve functionality and Web oriented lifetime management features.&nbsp; For some details on the performance of Munq compared to the other IOC Containers see my previous article <a href="http://www.codeproject.com/Articles/43296/Introduction-to-Munq-IOC-Container-for-ASP-NET.aspx" title="Introduction to Munq IOC Container for ASP.NET [Technical Blog]">Introduction to Munq IOC Container for ASP.NET</a>. You can also get someone else&rsquo;s opinion at <a href="http://omaralzabir.com/munq-is-for-web-unity-is-for-enterprise/" title="Munq is for web, Unity is for Enterprise">Munq is for web, Unity is for Enterprise</a> by Omar AL Zabir, creator of <a href="http://dropthings.omaralzabir.com/" target="_blank">DropThings</a>.&nbsp; But just to recap, the relative performances of the IOC containers is show below, smaller is better.</p>

<p><img complete="true" src="https://c2yrta.bay.livefilestore.com/y1meGKH9GpyhOoG_mPQlsQECXmZ0wV_v18H20IPrxYYfXTCyo--oCX-N250TtpAtKs30_2Yh9_WPbUIjPfJjFUNXbmOyCWEfkSK6qSXSxj3Nv6Cc3sU2oF6s_V-m72TOjL0qPP-nPfflt9snVylRu8p0A/image14.png" style="DISPLAY: block; FLOAT: none; MARGIN-LEFT: auto; MARGIN-RIGHT: auto" /></p>

<h2>Using Munq IocContainer</h2>

<p><a href="https://moxcsw.bay.livefilestore.com/y1m732mpRnovjwyaAzOX0tFaEpnkCt1vFcHwait7kTXUlovZx43S6dC5tOdvhF0Zthw-GMDb4UjGqL9-rqbvqGbPeT2rclYQIYPyfEFE8NCIzLY_BYH_TMinP0oiiBskXAifIMSX5XYr0FnEaXI36OLFA/image3.png" rel="WLPP"><img alt="image" border="0" complete="true" height="303" src="https://moxcsw.bay.livefilestore.com/y1mxLEJ9dx9sfrCEzBkZaT7gUqSCGnHQ1plErEDH9dDE9yBG7tz9YIkzC0pGniDJ-FRjvV1IhL5vikXGyO3dpezjOfrfbEApwdl8X18rqA-8-NhxZyWiv0eMNvQ-Onp8SMEy0qp_vFJRyMpyUyDwNSSEw/image_thumb2.png" style="BORDER-RIGHT-WIDTH: 0px; MARGIN: 0px 0px 10px 10px; DISPLAY: inline; BORDER-TOP-WIDTH: 0px; BORDER-BOTTOM-WIDTH: 0px; BORDER-LEFT-WIDTH: 0px" title="image" width="254" /></a>Version 2 of Munq has been refactored into two DLLs.&nbsp; The first, <em><strong>Munq.Interfaces</strong>,</em> contains only the interfaces required to use an instance of the <strong><em>IIocContainer</em></strong>, an IRegistration object (returned from one the <strong><em>IIocContainer.Register</em></strong> methods), or create your own lifetime manager.&nbsp; By programming to the interfaces, only the main application requires a hard dependency on the Munq IOC Container.&nbsp; Other DLLs can be passed the container instance as an<strong> <em>IIocContainer</em></strong>.&nbsp; This will allow the replacement of the IOC Container with another implementation if desired.</p>

<h6>The second DLL, <em><strong>Munq.IocContainer</strong></em> contains the implementation of the Munq IOC Container, the standard Lifetime Managers, a configuration loader, and a Controller Factory for ASP.NET MVC.</h6>

<h3>Registering Factory Methods</h3>

<p>Munq allows you to <strong><em>Register</em></strong> a named or unnamed factory method using Generic or Non-Generic methods, for a total of 4 different signatures for the Register method.&nbsp; Similarly, there are 4 methods for registering a pre-constructed instance to always be returned on a <strong><em>Resolve</em></strong> request.</p>

<div>
<pre style="BORDER-BOTTOM-STYLE: none; TEXT-ALIGN: ; PADDING-BOTTOM: 0px; LINE-HEIGHT: 12pt; BACKGROUND-COLOR: #f4f4f4; MARGIN: 0em; BORDER-LEFT-STYLE: none; PADDING-LEFT: 0px; WIDTH: 100.1%; PADDING-RIGHT: 0px; FONT-FAMILY: 'Courier New', courier, monospace; DIRECTION: ltr; BORDER-TOP-STYLE: none; HEIGHT: 183px; COLOR: black; BORDER-RIGHT-STYLE: none; FONT-SIZE: 10pt; OVERFLOW: visible; PADDING-TOP: 0px">
//Register
 IRegistration Register(string name, Type type, Func&lt;IIocContainer, object&gt; func);
 IRegistration Register(Type type, Func&lt;IIocContainer, object&gt; func);
 IRegistration Register&lt;TType&gt;(Func&lt;IIocContainer, TType&gt; func) where TType : class;
 IRegistration Register&lt;TType&gt;(string name, Func&lt;IIocContainer, TType&gt; func) where TType : class;

 //Register Instance
 IRegistration RegisterInstance(string name, Type type, object instance);
 IRegistration RegisterInstance(Type type, object instance);
 IRegistration RegisterInstance&lt;TType&gt;(string name, TType instance) where TType : class;
 IRegistration RegisterInstance&lt;TType&gt;(TType instance) where TType : class;
</pre>
</div>

<p>All the <strong><em>RegisterXXX</em></strong> methods return an object that implements the <strong><em>IRegistration</em></strong> interface.&nbsp; This interface allows you to get information about the registration, specify a <em><strong>LifetimeManger</strong></em>, of invalidate any cached instances.</p>

<div>
<pre style="BORDER-BOTTOM-STYLE: none; TEXT-ALIGN: ; PADDING-BOTTOM: 0px; LINE-HEIGHT: 12pt; BACKGROUND-COLOR: #f4f4f4; MARGIN: 0em; BORDER-LEFT-STYLE: none; PADDING-LEFT: 0px; WIDTH: 100.1%; PADDING-RIGHT: 0px; FONT-FAMILY: 'Courier New', courier, monospace; DIRECTION: ltr; BORDER-TOP-STYLE: none; HEIGHT: 157px; COLOR: black; BORDER-RIGHT-STYLE: none; FONT-SIZE: 10pt; OVERFLOW: visible; PADDING-TOP: 0px">
public interface IRegistration
{
    string Name         { get; }
    string Key          { get; }
    Type   ResolvesTo   { get; }
    IRegistration WithLifetimeManager(ILifetimeManager manager);
    void InvalidateInstanceCache();
}

</pre>
</div>

<h3>Obtaining an Instance from the IocContainer</h3>

<p>The user asks the container to create, or serve up a cached instance, of an implementation of an interface by calling one of the <strong><em>Resolve</em></strong> methods.&nbsp; For example:</p>

<div>
<pre style="BORDER-BOTTOM-STYLE: none; TEXT-ALIGN: ; PADDING-BOTTOM: 0px; LINE-HEIGHT: 12pt; BACKGROUND-COLOR: #f4f4f4; MARGIN: 0em; BORDER-LEFT-STYLE: none; PADDING-LEFT: 0px; WIDTH: 100%; PADDING-RIGHT: 0px; FONT-FAMILY: 'Courier New', courier, monospace; DIRECTION: ltr; BORDER-TOP-STYLE: none; COLOR: black; BORDER-RIGHT-STYLE: none; FONT-SIZE: 10pt; OVERFLOW: visible; PADDING-TOP: 0px">
// getting the class instance from the container    
IMyClass instance = MyApp.Container.Resolve&lt;IMyClass&gt;();    
instance.MethodA(42);</pre>
<br />
The container returns a instance of the required type, with all its dependencies resolved and fully initialized.</div>

<div>
<pre style="BORDER-BOTTOM-STYLE: none; TEXT-ALIGN: ; PADDING-BOTTOM: 0px; LINE-HEIGHT: 12pt; BACKGROUND-COLOR: #f4f4f4; MARGIN: 0em; BORDER-LEFT-STYLE: none; PADDING-LEFT: 0px; WIDTH: 100%; PADDING-RIGHT: 0px; FONT-FAMILY: 'Courier New', courier, monospace; DIRECTION: ltr; BORDER-TOP-STYLE: none; COLOR: black; BORDER-RIGHT-STYLE: none; FONT-SIZE: 10pt; OVERFLOW: visible; PADDING-TOP: 0px">
//Resolve
object Resolve(string name, Type type);
object Resolve(Type type);
TType Resolve&lt;TType&gt;() where TType : class;
TType Resolve&lt;TType&gt;(string name) where TType : class;
</pre>
</div>

<div>In many cases, you may not wish to actually create the instance due to the cost of construction, or use of scarce resources.&nbsp; Instead, you want to delay the construction until you need it.&nbsp; For these cases, call one of the <strong><em>LazyResolve</em></strong> methods.&nbsp;</div>

<div>
<pre style="BORDER-BOTTOM-STYLE: none; TEXT-ALIGN: ; PADDING-BOTTOM: 0px; LINE-HEIGHT: 12pt; BACKGROUND-COLOR: #f4f4f4; MARGIN: 0em; BORDER-LEFT-STYLE: none; PADDING-LEFT: 0px; WIDTH: 100.1%; PADDING-RIGHT: 0px; FONT-FAMILY: 'Courier New', courier, monospace; DIRECTION: ltr; BORDER-TOP-STYLE: none; HEIGHT: 99px; COLOR: black; BORDER-RIGHT-STYLE: none; FONT-SIZE: 10pt; OVERFLOW: visible; PADDING-TOP: 0px">
//Lazy Resolve
Func&lt;object&gt; LazyResolve(string name, Type type);
Func&lt;object&gt; LazyResolve(Type type);
Func&lt;TType&gt; LazyResolve&lt;TType&gt;() where TType : class;
Func&lt;TType&gt; LazyResolve&lt;TType&gt;(string name) where TType : class;
</pre>
</div>

<div>These will return a delegate that will get the instance from the container, when and if you need it.</div>

<div>
<pre style="BORDER-BOTTOM-STYLE: none; TEXT-ALIGN: ; PADDING-BOTTOM: 0px; LINE-HEIGHT: 12pt; BACKGROUND-COLOR: #f4f4f4; MARGIN: 0em; BORDER-LEFT-STYLE: none; PADDING-LEFT: 0px; WIDTH: 100%; PADDING-RIGHT: 0px; FONT-FAMILY: 'Courier New', courier, monospace; DIRECTION: ltr; BORDER-TOP-STYLE: none; COLOR: black; BORDER-RIGHT-STYLE: none; FONT-SIZE: 10pt; OVERFLOW: visible; PADDING-TOP: 0px">
// Example of LazyResolve
Func&lt;MyClass&gt; lazyLoader = Container.LazyResolve&lt;MyClass&gt;();
// do stuff
   ...
if (INeedMyClass)
{
   using(MyClass myClass = lazyLoader())
   {
      // do stuff with myClass
        ...
   }
}</pre>
</div>

<h3>Initializing the IOC Container</h3>

<p>Initialization of the IOC container can be performed in a number of ways, but usually occurs in the Application_Start of the Global.asax file for ASP.NET applications.&nbsp; Out of the box, Munq supports:</p>

<ul>
	<li>initialization by code</li>
	<li>automatic discovery and registration</li>
</ul>

<p>Typically, applications will use a combination of both.&nbsp; For example, the code below has an InitializeIOC method which calls the <strong><em>ConfigurationLoader.FindAndRegisterDependencies</em></strong> method and also the <strong><em>Register&lt;..&gt;</em></strong> method.</p>

<p>The <strong><em>FindAndRegisterDependencies</em></strong> method is passed the <strong><em>IIocContainer</em></strong> and does:</p>

<ol>
	<li>Searches the bin directory for any classes that implement the <strong><em>IMunqConfig</em></strong> interface.</li>
	<li>For each of these classes it creates an instance and calls the <strong><em>RegisterIn</em></strong> method, passing in the container.</li>
	<li>These methods allow the module to register its classes and dependencies with the container.</li>
</ol>

<p>The explicit registration allows the factory method and its dependencies to be registered.&nbsp; Examine the Register&lt;IController&gt;(&ldquo;Account&rdquo;, &hellip;) statement below.&nbsp; The line translated into English says, &ldquo;When asked for an instance of the IController interface, named &ldquo;Account&rdquo;, create an instance by calling the constructor.&nbsp; Pass in to the constructor instances of IFormsAuthentication and IMembershipService, both of which are also resolved by the container, as are any of their dependencies.&rdquo;</p>

<div>
<pre style="BORDER-BOTTOM-STYLE: none; TEXT-ALIGN: ; PADDING-BOTTOM: 0px; LINE-HEIGHT: 12pt; BACKGROUND-COLOR: #f4f4f4; MARGIN: 0em; BORDER-LEFT-STYLE: none; PADDING-LEFT: 0px; WIDTH: 100%; PADDING-RIGHT: 0px; FONT-FAMILY: 'Courier New', courier, monospace; DIRECTION: ltr; BORDER-TOP-STYLE: none; COLOR: black; BORDER-RIGHT-STYLE: none; FONT-SIZE: 10pt; OVERFLOW: visible; PADDING-TOP: 0px">
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



         ConfigurationLoader.FindAndRegisterDependencies(IOC);



         // Register the Controllers
         IOC.Register&lt;IController&gt;(&quot;Home&quot;, ioc =&gt; new HomeController());
         IOC.Register&lt;IController&gt;(&quot;Account&quot;,
                 ioc =&gt; new AccountController(ioc.Resolve&lt;IFormsAuthentication&gt;(),
                                              ioc.Resolve&lt;IMembershipService&gt;())
         );
     }
 }

</pre>
</div>

<h3>Lifetime Management</h3>

<p>Lifetime Management functionality has not changed since version 1, although the implementation has.</p>

<p>&nbsp;</p>

<p>Lifetime Managers allow you to modify the behaviour of the container as to how it resolves instances, and what is the lifetime of the instance.&nbsp; Munq has a set of Lifetime Managers designed for web applications.&nbsp; These are described below.</p>

<p><strong>Warning</strong>: if you used the <code>RegisterInstance</code> method, then the same instance will be returned regardless of which lifetime manager is used.</p>

<h4>AlwaysNewLifetime</h4>

<p>This lifetime manager&rsquo;s behaviour is to always return a new instance when the <code>Resolve</code> method is called by executing the factory method.&nbsp; This is the default behaviour.</p>

<h4>ContainerLifetime</h4>

<p>This lifetime manager&rsquo;s behaviour is to always return a the same instance when the <code>Resolve</code> method is called by executing the factory method.&nbsp; The instance is cached in the container itself.</p>

<h4>SessionLifetime</h4>

<p>This lifetime manager&rsquo;s behaviour is to always return a attempt to retrieve the instance from <code>Session</code> when the <code>Resolve</code> method is called.&nbsp; If the instance does not exist in <code>Session</code>, the a new instance is created by executing the factory method, and storing it in the <code>Session</code>.</p>

<h4>RequestLifetime</h4>

<p>This lifetime manager&rsquo;s behaviour is to always return a attempt to retrieve the instance from <code>Request.Items </code>when the <code>Resolve </code>method is called.&nbsp; If the instance does not exist in <code>Request.Items</code>, the a new instance is created by executing the factory method, and storing it in the <code>Request.Items</code>.</p>

<h4>CachedLifetime</h4>

<p>This lifetime manager&rsquo;s behaviour is to always return a attempt to retrieve the instance from <code>Cache</code> when the <code>Resolve</code> method is called.&nbsp; If the instance does not exist in <code>Cache</code>, the a new instance is created by executing the factory method, and storing it in the <code>Cache</code>. CacheDependencies and Sliding or Absolute Timeouts can be applied to the the CachedLifetimeManager.</p>

<h3>Conclusion</h3>

<p>This article was just a brief overview of the functionality and performance of the Munq IocContainer.&nbsp; I will examine different aspects of using the Munq.IocContainer in other articles in this series, so stay tuned.</p>

<ul>
</ul>
