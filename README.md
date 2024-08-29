

<!DOCTYPE HTML>
<html>
<head>
</head>
<body>
<!--
HTML for article "Introduction to Munq IOC Container for ASP.NET" by Matthew Dennis
URL: https://www.codeproject.com/KB/aspnet/43296.aspx
Copyright 2009 by Matthew Dennis
All formatting, additions and alterations Copyright © CodeProject, 1999-2024
-->

<hr class="Divider subdue" />
<div>




<!-- Start Article -->
<span id="ArticleContent">


<h2>Introduction</h2>

<p>This is the first in a series of articles about using the Munq DI IOC Container and IOC containers in general.</p>

<p>Inversion of Control (IOC) is a pattern which decouples the use of an interface from the concrete implementation of that interface. By eliminating this coupling:</p>

<ul>
<li>the code can be tested by using special test implementations of the interface. </li><li>fewer DLLs need to be deployed on each tier of a multi-tiered deployment. </li><li>project compilation may be sped up as there are fewer project dependencies. </li><li>alternate implementations of the interface can be configured for use without requiring changes to the main application code. This could mean changing from MSSQL to Oracle with a simple configuration file change. </li></ul>

<p>An IOC Container is a pattern which allows the application to an instance of the interface without knowing the specific concrete class that will be used.</p>

<p>Munq DI IOC Container is the smallest, fastest IOC container that I know of. It is faster than any of the big players, Unity, Windsor Castle, Ninject, StructureMap, etc. See the graph below. Additionally, it has been designed for the web developer, with lifetime management that includes Request, Session, and Cached lifetime managers. </p>

<p>This article will provide you with a detailed understanding of the Munq IOC Container with: </p>

<ul>
<li>a performance graph to grab your attention </li><li>API documentation </li></ul>

<p>Additional articles will demonstrate the usage of the Munq DI IOC Container with real world examples including using Munq with ASP.NET MVC, a tour of the Munq code, and things to watch out for when using IOC Containers. </p>

<h2>Background</h2>

<p>Earlier this year, I watched the very interesting <a href="http://www.clariusconsulting.net/blogs/kzu/archive/2009/02/02/116399.aspx">Funq Screencast Series</a> by <a href="http://www.clariusconsulting.net/blogs/kzu/" target="_blank">Daniel Cazzulino (Kzu)</a>. This nine part series detail the TDD development of a small, fast IOC Container called <a href="http://funq.codeplex.com/" target="_blank">Funq</a> which was destined to evolve into the <a href="http://www.clariusconsulting.net/blogs/kzu/archive/2009/04/20/142789.aspx" target="_blank">ContainerModel</a> for the <a href="http://mobile.codeplex.com/" target="_blank">Patterns &amp; Practices: Mobile Application Blocks</a>. </p>

<p>After downloading the code, I discovered a number of improvements which both simplified the code and significantly improved the performance. After several discussions, Kzu made me a contributor and several of my ideas and code were incorporated into the codebase. </p>

<p>Unfortunately, Kzu’s target for Funq was the mobile development space, while I am more involved in ASP.NET Webform and MVC application development. I didn’t need some of the features of Funq such as Container Hierarchies and Initializers, and I did need lifetime management to include web oriented styles including Request, Session, and Cached lifetime management. Thus Munq was conceived. </p>

<p>The goals for this development were: </p>

<ol>
<li>Speed. High volume websites need to minimize the amount of work each page request needs to execute. Even 1/10<sup>th</sup> of a second can make a difference of a user’s perception of the site. </li><li>Simplicity. Munq does one thing, but does it well. It resolves requests for instances of Types by executing Factories that have been previously registered in the container. </li><li>Provide Web Lifetime Management. Windows applications can get by with a container that has the option of creating a new instance each request, or re-use the same instance. In the web application world, objects can have lifetimes that only span the current Request, the user’s Session, or be Cached reduce database load and access delays. </li></ol>

<p>The latest release of Munq is available on <a href="http://www.codeplex.com/" target="_blank">CodePlex</a> at <a href="http://munq.codeplex.com/">http://munq.codeplex.com</a>.</p>

<h2>Performance</h2>

<p>While developing Funq, Kzu and I created a small performance measuring application to compare the relative overhead of creating new instances form various IOC containers.</p>

<p>This test used the IOC Containers to build up a “<code>WebApp</code>” which implemented the <code>IWebService</code> interface which has the dependencies shown in the following diagram:</p>

<p><a href="https://c2yrta.bay.livefilestore.com/y1myICqP2PgA016v8SihJ-cAk5bVpCM-3vxgEQyFh27ptEa8f6qD3F5frqqRuNfcZUY6wNzfNxiBoscawYAY2-ijYn5FJilmEdo8xARPY6BNdZf1k-rkm8FFjVBygC-XwOvueUSq-kFPv-28TELMVR4rg/image13.png" rel="WLPP"><img title="Sample App Dependencies" style="float: none; margin-left: auto; margin-right: auto" height="426" alt="Sample App Dependencies" hspace="0" src="https://c2yrta.bay.livefilestore.com/y1moMT37yD867AS346FpE_MIKu_JX2dZtgoyPB-nASagmYV7Tqb6dMHY9E3gybmLPkFImgXyqTe7Z2-Zta4-i3l00zWDXJFx7NecHpVbM3Cf7etyTD4C-qqbFXxljFSdsp2uSfA_UDUHpgWvRzX-PKPmQ/image_thumb9.png" width="528" border="0"></a> </p>

<p>All Containers were configured to return new instances for all interfaces except <code>ILogger </code>which was a shared instance. Running 10000 iterations for each use case had the following results.</p>


<table class="ArticleTable" cellspacing="0" cellpadding="4" width="285" align="center" border="1"><thead><tr><td valign="top" width="130"><strong>Container</strong></td><td valign="top" width="153"><strong>Ticks/Iteration</strong></td></tr></thead><tbody><tr><td valign="top" width="130">None</td><td valign="top" width="153">5.1746</td></tr><tr><td valign="top" width="130">Munq</td><td valign="top" width="153">68.3385</td></tr><tr><td valign="top" width="130">Funq</td><td valign="top" width="153">76.2779</td></tr><tr><td valign="top" width="130">Unity</td><td valign="top" width="153">613.0442</td></tr><tr><td valign="top" width="130">Autofac</td><td valign="top" width="153">877.035</td></tr><tr><td valign="top" width="130">StructureMap</td><td valign="top" width="153">280.9433</td></tr><tr><td valign="top" width="130">Ninject</td><td valign="top" width="153">4122.1138</td></tr><tr><td valign="top" width="130">Ninject2</td><td valign="top" width="153">5059.9001</td></tr><tr><td valign="top" width="130">Windsor</td><td valign="top" width="153">4206.1035</td></tr></tbody></table>

<p>This is shown in the graph below, smaller is better.</p>

<p><a href="https://c2yrta.bay.livefilestore.com/y1meGKH9GpyhOoG_mPQlsQECXmZ0wV_v18H20IPrxYYfXTCyo--oCX-N250TtpAtKs30_2Yh9_WPbUIjPfJjFUNXbmOyCWEfkSK6qSXSxj3Nv6Cc3sU2oF6s_V-m72TOjL0qPP-nPfflt9snVylRu8p0A/image14.png" rel="WLPP"><img title="IOC Container Performance" style="float: none; margin-left: auto; margin-right: auto" height="424" alt="IOC Container Performance" hspace="0" src="https://c2yrta.bay.livefilestore.com/y1mHHXFUR6jcSbgoL2sUoAyhNCSEXGzNWJCcE8lNTeDQQrcjlArXmCVlEpStqrIlL0inR3tWPj-6A8s1tIeT-BdlO9dxg0YhJw32PK2aQVljS0PEHU7SJBmdyd7X6xKqFeM71gbQDLXSuPycIyHFJArZg/image_thumb10.png" width="587" border="0"></a> </p>

<p>The code to register the Factories is as follows: </p>

<pre class='lang-csharp' data-language='cs' data-allowShrink='True' data-collapse='False' data-showheader='false'>
ILifetimeManager lifetime = new ContainerLifetime();

	Container container = new Container();

	container.Register&lt;IWebservice&gt;(
		c =&gt; new WebService(
			c.Resolve&lt;IAuthenticator&gt;(),
			c.Resolve&lt;IStockquote&gt;()));

	container.Register&lt;IAuthenticator&gt;(
		c =&gt; new Authenticator(
			c.Resolve&lt;ILogger&gt;(),
			c.Resolve&lt;IErrorhandler&gt;(),
			c.Resolve&lt;IDatabase&gt;()));

	container.Register&lt;IStockquote&gt;(
		c =&gt; new StockQuote(
			c.Resolve&lt;ILogger&gt;(),
			c.Resolve&lt;IErrorhandler&gt;(),
			c.Resolve&lt;IDatabase&gt;()));

	container.Register&lt;IDatabase&gt;(
		c =&gt; new Database(
			c.Resolve&lt;ILogger&gt;(),
			c.Resolve&lt;IErorhandler&gt;()));

	container.Register&lt;IErrorhandler&gt;(
		c =&gt; new ErrorHandler(c.Resolve&lt;ILogger&gt;()));

	container.RegisterInstance&lt;ILlogger&gt;(new Logger())
		.WithLifetimeManager(lifetime);</pre>

<p>Resolving the instance: </p>

<pre class='lang-csharp' data-language='cs' data-allowShrink='True' data-collapse='False' data-showheader='false'>
var webApp = container.Resolve&lt;IWebservice&gt;();
webApp.Execute();
</pre>

<h2>Using the Code</h2>

<p>Using the container is relatively simple. The basic steps are:</p>

<ol>
<li>Create the Container. </li><li>Register the factory delegates for the Interfaces and/or Classes. </li><li>Resolve instances by calling <code>Resolve </code>methods of the container. </li></ol>

<h3>Creating the IOC Container</h3>

<p>The container is usually created when the application first starts. In a web application, the container would typically be created in the <code>Application_Start </code>and stored in a field of the derived <code>Application </code>class or a <code>static </code>variable.</p>

<pre class='lang-csharp' data-language='cs' data-allowShrink='True' data-collapse='False' data-showheader='false'>
Container container = new Container();</pre>

<h3>Registering Type Factories</h3>

<p>Registering the Type Factories associates a type to be resolved with a function that will return an instance of that type. This method is called when the container is asked to return an instance of the type.</p>

<p>Munq has four ways of registering type factory functions. These functions can be anything which has the correct signature and are not limited to delegates of the form</p>

<pre class='lang-csharp' data-language='cs' data-allowShrink='True' data-collapse='False' data-showheader='false'>
c=&gt; new MyType()</pre>

<p>but could be </p>

<pre class='lang-csharp' data-language='cs' data-allowShrink='True' data-collapse='False' data-showheader='false'>
c =&gt; CreateAndInitialzeMyType(c.Resolve&lt;IOne&gt;(), c.Resolve&lt;ITwo&gt;)</pre>

<p>The first is using the type-safe generic <code>Register </code>methods. There are versions for both named and un-named registrations. Both take as a parameter, a delegate which takes a <code>Container </code>as its single parameter and returns an instance of the type.</p>

<pre class='lang-csharp' data-language='cs' data-allowShrink='True' data-collapse='False' data-showheader='false'>
public IRegistration Register&lt;TType&gt;(Func&lt;Container, TType&gt; func)
public IRegistration Register&lt;TType&gt;(string name, Func&lt;Container, TType&gt;func)</pre>

<p>The second is a set of methods which allows the factory method to be registered by passing the type of be resolved, and a delegate which takes a <code>Container </code>as its single parameter and returns an <code>Object</code>. There are versions for named and un-named registrations. These methods would typically be used if the registration information was read from an external store such as a database, XML file, or the <em>web.config</em> file. </p>

<pre class='lang-csharp' data-language='cs' data-allowShrink='True' data-collapse='False' data-showheader='false'>
public IRegistration Register(Type type, Func&lt;Container, object&gt; func)
public IRegistration Register(string name, Type type, Func&lt;Container, object&gt; func)</pre>

<p>The third method is using the type-safe generic <code>RegisterInstance </code>methods. There are versions for both named and un-named registrations. Both take as a parameter, an instance of the type.</p>

<pre class='lang-csharp' data-language='cs' data-allowShrink='True' data-collapse='False' data-showheader='false'>
public IRegistration RegisterInstance&lt;TType&gt;(TType instance)
public IRegistration RegisterInstance&lt;TType&gt;(string name, TType instance)</pre>

<p>The fourth is a set of methods which allows the factory method to be registered by passing the type of be resolved, and an object to be returned when the type is resolved. There are versions for named and un-named registrations. These methods would typically be used if the registration information was read from an external store such as a database, XML file, or the <em>web.config</em> file. </p>

<pre class='lang-csharp' data-language='cs' data-allowShrink='True' data-collapse='False' data-showheader='false'>
public IRegistration RegisterInstance(Type type, object instance)
public IRegistration RegisterInstance(string name, Type type, object instance)</pre>

<h3>Getting an Instance from the Container</h3>

<p>Once an interface and function have been registered in the container, the application can retrieve an instance by asking the container to resolve the interface to the concrete implementation that was registered. Munq has two forms of the <code>Resolve </code>method, a type-safe version using Generics, and a version that takes a <code>Type </code>as a parameter and returns an <code>Object</code>. Both versions have named and un-named overloads.</p>

<pre class='lang-csharp' data-language='cs' data-allowShrink='True' data-collapse='False' data-showheader='false'>
public TType Resolve&lt;TType&gt;()
public TType Resolve&lt;TType&gt;(string name)

public object Resolve(Type type)
public object Resolve(string name, Type type)</pre>

<h3>Lazy Resolution</h3>

<p>There are situations where you may not wish to create the instance immediately. This may be because the instance uses a scarce resource or due to logic that may not require instantiation. For these cases, you can use the <code>LazyResolve </code>methods which returns a function that, when executed, performs the resolution and returns the instance.</p>

<p>Like the <code>Resolve </code>methods, the <code>LazyResolve </code>has two forms, a type-safe version using Generics, and a version that takes a <code>Type </code>as a parameter. Both versions have named and un-named overloads.</p>

<pre class='lang-csharp' data-language='cs' data-allowShrink='True' data-collapse='False' data-showheader='false'>
Func LazyResolve&lt;ttype&gt;() 
Func&lt;ttype&gt; LazyResolve&lt;ttype&lt;(string name)
 
Func&lt;Object&gt; LazyResolve(Type type) 
Func&lt;Object&gt; LazyResolve(string name, Type type)</pre>

<h3>What is this IRegistration Thing?</h3>

<p>You may have noticed that the <code>Registration </code>methods all return an object that implements the <code>IRegistration </code>interface. This interface allows the user to retrieve the internally generated ID for the registration and to specify the <code>LifetimeManager </code>to be used by the instance when it is resolved.</p>

<p>For example, if you need an <code>IShoppingCart</code> object to be stored in the users session, you would tell the registration to use the <code>SessionLifetime</code> manager:</p>

<pre class='lang-csharp' data-language='cs' data-allowShrink='True' data-collapse='False' data-showheader='false'>
ILifetimeManager lifetime = new SessionLifetime();
container.Register&lt;IShoppingCart&gt;(c =&gt; new MyShoppingCart())
         .WithLifetimeManager(lifetime);
</pre>

<p>Notice that this has been implemented in a fluent interface manner to allow changing of method calls. While there is currently only one method on this interface, any future methods will follow this pattern.</p>

<pre class='lang-csharp' data-language='cs' data-allowShrink='True' data-collapse='False' data-showheader='false'>
 public interface IRegistration
{
    string Id { get; }
    IRegistration WithLifetimeManager(ILifetimeManager manager);
}</pre>

<h3>Specifying the Default Lifetime Manager</h3>

<p>When the <code>Container </code>is first created, the default behaviour is to return a new instance for each <code>Resolve </code>method call. As shown above, this can be modified on a registration by registration basis. Additionally, you can specify which lifetime manager is to be used for any registration. For example, to always return the same instance for each call to <code>Resolve</code>, use the <code>ContainerLifetime</code> manager as shown below:</p>

<pre class='lang-csharp' data-language='cs' data-allowShrink='True' data-collapse='False' data-showheader='false'>
Container container = new Container();
ILifetimeManager lifetime = new ContainerLifetime();

container.UsesDefaultLifetimeManagerOf(lifetime);</pre>

<p>The method definition is:</p>

<pre class='lang-csharp' data-language='cs' data-allowShrink='True' data-collapse='False' data-showheader='false'>
public Container UsesDefaultLifetimeManagerOf(ILifetimeManager lifetimeManager)</pre>

<h3>Available Lifetime Managers</h3>

<p>Lifetime Managers allow you to modify the behaviour of the container as to how it resolves instances, and what is the lifetime of the instance. Munq has a set of Lifetime Managers designed for web applications. These are described below.</p>

<p><strong>Warning</strong>: if you used the <code>RegisterInstance</code> method, then the same instance will be returned regardless of which lifetime manager is used.</p>

<h4>AlwaysNewLifetime</h4>

<p>This lifetime manager’s behaviour is to always return a new instance when the <code>Resolve</code> method is called by executing the factory method. This is the default behaviour.</p>

<h4>ContainerLifetime</h4>

<p>This lifetime manager’s behaviour is to always return the same instance when the <code>Resolve</code> method is called by executing the factory method.</p>

<h4>SessionLifetime</h4>

<p>This lifetime manager’s behaviour is to always return an attempt to retrieve the instance from <code>Session</code> when the <code>Resolve</code> method is called. If the instance does not exist in <code>Session</code>, a new instance is created by executing the factory method, and storing it in the <code>Session</code>.</p>

<h4>RequestLifetime</h4>

<p>This lifetime manager’s behaviour is to always return an attempt to retrieve the instance from <code>Request.Items </code>when the <code>Resolve </code>method is called. If the instance does not exist in <code>Request.Items</code>, a new instance is created by executing the factory method, and storing it in the <code>Request.Items</code>.</p>

<h4>CachedLifetime</h4>

<p>This lifetime manager’s behaviour is to always return an attempt to retrieve the instance from <code>Cache</code> when the <code>Resolve</code> method is called. If the instance does not exist in <code>Cache</code>, a new instance is created by executing the factory method, and storing it in the <code>Cache</code>.</p>

<h2>Conclusion</h2>

<p>I have given you a brief overview of the Munq DI IOC and its API. I will be following this with additional articles with examples of using Munq and dissecting the Munq code. </p>

<!-- End Article -->

</div> 
</body>
</html>
