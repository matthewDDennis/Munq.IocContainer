<h2>Introduction</h2>

<p>Version 3 of the <strong>Munq IOC Container </strong>adds a number of features including documentation and NuGet packaging. The Munq source is maintained on <a alt="Munq project on CodePlex" href="http://munq.codeplex.com">CodePlex</a>. This article demonstrates how to integrate the Munq IOC Container into an ASP.NET MVC project using the NuGet package.</p>

<p>For my example, I will refactor the <code>AccountController </code>to use Dependency Injection and the Munq IOC Container. At the end, I will have:</p>

<ul>
	<li>Integrated the Munq IOC Container into an ASP.NET project.</li>
	<li>Refactored the <code>AccountController </code>to use Dependency Injection.</li>
	<li>Initialized the IOC Container with the required Registrations.</li>
	<li>Updated the Unit Tests.</li>
</ul>

<p>The final solution will allow you to easily replace the <code>IFormsAuthenticationService</code> and <code>IMembershipService</code> implementations through the IOC Container configuration. As an added bonus, you can replace the instance of the <code>MembershipProvider</code> used by the <code>AccountMembershipService</code>.</p>

<h2>Background</h2>

<p>Previously, I&#39;ve written about the Munq IOC container</p>

<ul>
	<li><a href="http://www.codeproject.com/Articles/83416/Munq-IocContainer-V2-Overview.aspx">Munq IocContainer V2 - Overview</a></li>
	<li><a href="http://www.codeproject.com/Articles/43296/Introduction-to-Munq-IOC-Container-for-ASP-NET.aspx">Introduction to Munq IOC Container for ASP.NET</a></li>
	<li><a href="http://www.codeproject.com/Articles/43354/Using-Munq-IOC-with-ASP-NET-MVC-2-Preview-2.aspx">Using Munq IOC with ASP.NET MVC 2 Preview 2</a></li>
	<li><a href="http://www.codeproject.com/Articles/83519/Munq-IOC-Container-Creating-a-Thread-Local-Storage.aspx">Munq IOC Container &ndash; Creating a Thread Local Storage Lifetime Manager</a></li>
</ul>

<p>As a result of the feedback and suggestion I have received, and the need to use the IOC Container with ASP.NET MVC3, I have released an updated version of Munq. In addition to the core IOC Container, there is an implementation of the ASP.NET MVC3 <code>IDependencyResolver</code> and the <code>Common Service Locator</code>. All three are available as NuGet packages.</p>

<p>Probably, the most important additions to the Munq IOC container are:</p>

<ul>
	<li>The automatic resolution of classes to the <code>public </code>constructor with the most parameters. This means that a class is resolved by the class type, it does not need to be registered with the container.</li>
	<li>Registration by types. Using the above, you can register a interface implementation in the form <code>container.Register&lt;IMyType, MyClass&gt;();</code></li>
</ul>

<h2>Show Me the Code</h2>

<p>The first thing to do is to create a MVC3 project, selecting the Internet Application option. Also select ASP.NET as the view engine, and check the box for unit tests. This creates a simple starting point that includes a Home page, an About page, and Form based authentication using SQL Express.</p>

<p>Build and run the application, just so we know it works. Also, run the unit test to verify they pass.</p>

<p>If we take a look at the <em>AccountController.cs</em> file, we see that the dependencies for this controller are wired up in the <code>Initialize </code>method:</p>

<pre lang="cs">
public IFormsAuthenticationService FormsService { get; set; }
public IMembershipService MembershipService { get; set; }

protected override void Initialize(RequestContext requestContext)
{
    if (FormsService == null)      { FormsService = new FormsAuthenticationService(); }
    if (MembershipService == null) { MembershipService = new AccountMembershipService(); }

    base.Initialize(requestContext);
}</pre>

<p>We want to change this to use Dependency Injection so we will remove this method and add a constructor which has the authentication and membership services as parameters. Also, the <code>FormsService</code> and <code>MembershipService</code> properties expose implementation details that should not be needed by users of the controller. We won&#39;t fix that right now, as it will break a number of the unit tests, and isn&#39;t important in showing how to get the Dependency Injection working.</p>

<p>A quick refactoring and we end up with:</p>

<pre lang="cs">
public IFormsAuthenticationService FormsService      { get; set; }
public IMembershipService          MembershipService { get; set; }

public AccountController(IFormsAuthenticationService formsService,
                                IMembershipService membershipService)
{
    FormsService      = formsService;
    MembershipService = membershipService;
}</pre>

<p>Build the solution and we get an error in the unit test stating that <code>AccountController</code> does not have a constructor with no parameters.</p>

<p>Modify the <code>GetAccountController</code> method in <em>AccountControllerTest.cs</em> to:</p>

<pre lang="cs">
private static AccountController GetAccountController()
{
    RequestContext requestContext = 
		new RequestContext(new MockHttpContext(), new RouteData());
    AccountController controller = 
		new AccountController(new MockFormsAuthenticationService(),
                  new MockMembershipService())
    {
        Url = new UrlHelper(requestContext),
    };
    controller.ControllerContext = new ControllerContext()
    {
        Controller = controller,
        RequestContext = requestContext
    };
    return controller;
}</pre>

<p>Also, the <code>AccountMembershipService</code> class, defined in <em><strong>Models/AccountModels.cs</strong></em> has a parameterless constructor. Remove it. Also change the other constructor as shown below:</p>

<pre lang="cs">
public class AccountMembershipService : IMembershipService
{
    private readonly MembershipProvider _provider;

    public AccountMembershipService(MembershipProvider provider)
    {
        _provider = provider;
    }

    ...</pre>

<p>We now have a successful build and all the tests still pass, but when we run it, we get an error when we click on the logon link. The error...</p>

<pre lang="text">
<strong>System.MissingMethodException: No parameterless constructor defined for this object.</strong></pre>

<p>...which as the stack trace explains is caused by the <code>AccountController</code>.</p>

<p>Now, we need to add the IOC Container. Fortunately, I have created a NuGet package for using the Munq IOC Container in MVC3. I am assuming that you have NuGet installed in your copy of Visual Studio. Right click on the <code>MuncMvc3Sample </code>project and select <strong>Add Library Package Reference ...</strong>. This will bring up the NuGet dialog. Search for Munq in the online packages. You will see three choices. Install the <code>Munq.MVC3</code> package.</p>

<p><a href="MunqNuget.PNG"><img alt="Munq IOC on NuGet - Click to enlarge image" border="0" height="423" hspace="0" src="MunqNuget_small.PNG" width="640" /></a></p>

<p>This installs and adds references to:</p>

<ul>
	<li><em>Munq.IocContainer.dll</em> (the Munq IOC Container)</li>
	<li><em>Munq.MVC3.dll</em> (contains the <code>MunqDependencyResolver</code> which implements the <code>System.Web.Mvc.IDependency</code> interface)</li>
	<li><em>WebActivator.dll</em> (allows the <code>MunqDependencyResolver</code> to be automatically wired up)</li>
</ul>

<p>Additionally, a directory <code>App_Start</code> is created and contains one file, <em><strong>MunqMvc3Startup.cs</strong></em>. This file contains the code to configure MVC3 to use the Munq IOC Container for its dependency resolution tasks.</p>

<pre lang="cs">
using System.Web.Mvc;
using Munq.MVC3;

[assembly: WebActivator.PreApplicationStartMethod
	(typeof(MunqMvc3Sample.App_Start.MunqMvc3Startup), &quot;PreStart&quot;)]
namespace MunqMvc3Sample.App_Start {
    public static class MunqMvc3Startup {
        public static void PreStart() {
            DependencyResolver.SetResolver(new MunqDependencyResolver());
            var ioc = MunqDependencyResolver.Container;

            // TODO: Register Dependencies
            // ioc.Register&lt;IMyRepository, MyRepository&gt;();
        }
    }
}</pre>

<p>Make sure the Development Web Server is stopped, so the startup code is executed. Now try and build and run. We still get the error. This is because we haven&#39;t Registered the implementations for <code>IFormsService</code> and <code>IMembershipService</code>. Because of this, MVC falls back to attempting to create the <code>AccountController</code> with the parameterless constructor, which does not exist. In <em>MunqMvc3Startup.cs</em>, register the services.</p>

<pre lang="cs">
using System.Web.Mvc;
using Munq.MVC3;
using MunqMvc3Sample.Models;

[assembly: WebActivator.PreApplicationStartMethod(
    typeof(MunqMvc3Sample.App_Start.MunqMvc3Startup), &quot;PreStart&quot;)]

namespace MunqMvc3Sample.App_Start {
    public static class MunqMvc3Startup {
        public static void PreStart() {
            DependencyResolver.SetResolver(new MunqDependencyResolver());
            var ioc = MunqDependencyResolver.Container;

            // TODO: Register Dependencies
            // ioc.Register&lt;IMyRepository, MyRepository&gt;();

            // setup AccountController&#39;s dependencies
            ioc.Register&lt;IFormsAuthenticationService, FormsAuthenticationService&gt;();
            ioc.Register&lt;IMembershipService, AccountMembershipService&gt;();

            // AccountMembershipService needs a MembershipProvider
            ioc.Register&lt;MembershipProvider&gt;(c =&gt; Membership.Provider);
        }
    }
}</pre>

<p>Again, stop the Development Server and then build and run. Now, when you click run, the login form is displayed. MvC3 is now using Munq to resolve the dependencies. Notice that we did not have to register the <code>AccountController</code> class itself. For classes, Munq will attempt to resolve using the constructor with the most parameters. This is exactly what MVC3 requires, and why the feature was added to Munq.</p>

<p><strong>This means, if you add or delete parameters (dependencies) to a Controller&#39;s constructor, it will still be resolved as long as the dependencies are Registered or are classes. </strong></p>

<p>Next, I want to refactor out <code>FormsService</code> and <code>MembershipService</code> properties from the <code>AccountController</code> class. I&#39;m using CodeRush, so the refactoring is pretty easy.</p>

<pre lang="cs">
private IFormsAuthenticationService _formsService;
private IMembershipService          _membershipService;

public AccountController(IFormsAuthenticationService formsService,
                                    IMembershipService membershipService)
{
    _formsService      = formsService;
    _membershipService = membershipService;
}    </pre>

<p>Building now results in a number of errors in the unit test that were using this property. I will not detail the changes here, but you can see them in the source files included.</p>

<h2>Conclusion</h2>

<p>The result of this example is a Visual Studio project that can be used as a template for future development with ASP.NET MVC3 and the Munq IOC Container.</p>

<h2>History</h2>

<ul>
	<li>First Release</li>
</ul>

<div class="ms-editor-squiggler" style="color: initial; font: initial; font-feature-settings: initial; font-kerning: initial; font-optical-sizing: initial; font-variation-settings: initial; forced-color-adjust: initial; text-orientation: initial; text-rendering: initial; -webkit-font-smoothing: initial; -webkit-locale: initial; -webkit-text-orientation: initial; -webkit-writing-mode: initial; writing-mode: initial; zoom: initial; place-content: initial; place-items: initial; place-self: initial; alignment-baseline: initial; animation: initial; appearance: initial; aspect-ratio: initial; backdrop-filter: initial; backface-visibility: initial; background: initial; background-blend-mode: initial; baseline-shift: initial; block-size: initial; border-block: initial; border: initial; border-radius: initial; border-collapse: initial; border-end-end-radius: initial; border-end-start-radius: initial; border-inline: initial; border-start-end-radius: initial; border-start-start-radius: initial; inset: initial; box-shadow: initial; box-sizing: initial; break-after: initial; break-before: initial; break-inside: initial; buffered-rendering: initial; caption-side: initial; caret-color: initial; clear: initial; clip: initial; clip-path: initial; clip-rule: initial; color-interpolation: initial; color-interpolation-filters: initial; color-rendering: initial; color-scheme: initial; columns: initial; column-fill: initial; gap: initial; column-rule: initial; column-span: initial; contain: initial; contain-intrinsic-size: initial; content: initial; content-visibility: initial; counter-increment: initial; counter-reset: initial; counter-set: initial; cursor: initial; cx: initial; cy: initial; d: initial; display: block; dominant-baseline: initial; empty-cells: initial; fill: initial; fill-opacity: initial; fill-rule: initial; filter: initial; flex: initial; flex-flow: initial; float: initial; flood-color: initial; flood-opacity: initial; grid: initial; grid-area: initial; height: 0px; hyphens: initial; image-orientation: initial; image-rendering: initial; inline-size: initial; inset-block: initial; inset-inline: initial; isolation: initial; letter-spacing: initial; lighting-color: initial; line-break: initial; list-style: initial; margin-block: initial; margin: initial; margin-inline: initial; marker: initial; mask: initial; mask-type: initial; max-block-size: initial; max-height: initial; max-inline-size: initial; max-width: initial; min-block-size: initial; min-height: initial; min-inline-size: initial; min-width: initial; mix-blend-mode: initial; object-fit: initial; object-position: initial; offset: initial; opacity: initial; order: initial; origin-trial-test-property: initial; orphans: initial; outline: initial; outline-offset: initial; overflow-anchor: initial; overflow-wrap: initial; overflow: initial; overscroll-behavior-block: initial; overscroll-behavior-inline: initial; overscroll-behavior: initial; padding-block: initial; padding: initial; padding-inline: initial; page: initial; page-orientation: initial; paint-order: initial; perspective: initial; perspective-origin: initial; pointer-events: initial; position: initial; quotes: initial; r: initial; resize: initial; ruby-position: initial; rx: initial; ry: initial; scroll-behavior: initial; scroll-margin-block: initial; scroll-margin: initial; scroll-margin-inline: initial; scroll-padding-block: initial; scroll-padding: initial; scroll-padding-inline: initial; scroll-snap-align: initial; scroll-snap-stop: initial; scroll-snap-type: initial; shape-image-threshold: initial; shape-margin: initial; shape-outside: initial; shape-rendering: initial; size: initial; speak: initial; stop-color: initial; stop-opacity: initial; stroke: initial; stroke-dasharray: initial; stroke-dashoffset: initial; stroke-linecap: initial; stroke-linejoin: initial; stroke-miterlimit: initial; stroke-opacity: initial; stroke-width: initial; tab-size: initial; table-layout: initial; text-align: initial; text-align-last: initial; text-anchor: initial; text-combine-upright: initial; text-decoration: initial; text-decoration-skip-ink: initial; text-indent: initial; text-overflow: initial; text-shadow: initial; text-size-adjust: initial; text-transform: initial; text-underline-offset: initial; text-underline-position: initial; touch-action: initial; transform: initial; transform-box: initial; transform-origin: initial; transform-style: initial; transition: initial; user-select: initial; vector-effect: initial; vertical-align: initial; visibility: initial; -webkit-app-region: initial; border-spacing: initial; -webkit-border-image: initial; -webkit-box-align: initial; -webkit-box-decoration-break: initial; -webkit-box-direction: initial; -webkit-box-flex: initial; -webkit-box-ordinal-group: initial; -webkit-box-orient: initial; -webkit-box-pack: initial; -webkit-box-reflect: initial; -webkit-highlight: initial; -webkit-hyphenate-character: initial; -webkit-line-break: initial; -webkit-line-clamp: initial; -webkit-mask-box-image: initial; -webkit-mask: initial; -webkit-mask-composite: initial; -webkit-perspective-origin-x: initial; -webkit-perspective-origin-y: initial; -webkit-print-color-adjust: initial; -webkit-rtl-ordering: initial; -webkit-ruby-position: initial; -webkit-tap-highlight-color: initial; -webkit-text-combine: initial; -webkit-text-decorations-in-effect: initial; -webkit-text-emphasis: initial; -webkit-text-emphasis-position: initial; -webkit-text-fill-color: initial; -webkit-text-security: initial; -webkit-text-stroke: initial; -webkit-transform-origin-x: initial; -webkit-transform-origin-y: initial; -webkit-transform-origin-z: initial; -webkit-user-drag: initial; -webkit-user-modify: initial; white-space: initial; widows: initial; width: initial; will-change: initial; word-break: initial; word-spacing: initial; x: initial; y: initial; z-index: initial;">&nbsp;</div>
