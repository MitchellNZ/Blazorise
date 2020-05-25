﻿#region Using directives
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Blazorise.Stores;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
#endregion

namespace Blazorise
{
    public partial class Bar : BaseComponent, IBreakpointActivator
    {
        #region Members

        private bool lastBrokenState;

        private Breakpoint breakpoint = Breakpoint.None;

        private ThemeContrast themeContrast = ThemeContrast.Light;

        private Alignment alignment = Alignment.None;

        private BarMode initialMode = BarMode.Horizontal;

        private BarCollapseMode collapseMode = BarCollapseMode.Hide;

        private Background background = Background.None;

        private DotNetObjectReference<BreakpointActivatorAdapter> dotNetObjectRef;

        private BarStore store = new BarStore
        {
            Mode = BarMode.Horizontal
        };

        #endregion

        #region Methods

        protected override async Task OnInitializedAsync()
        {
            if ( Mode != BarMode.Horizontal )
            {
                lastBrokenState = BreakpointActivatorAdapter.IsBroken( this, await JSRunner.GetBreakpoint() );
                Visible = !lastBrokenState;
                ToggleMode();
            }

            await base.OnInitializedAsync();
        }

        protected override async Task OnFirstAfterRenderAsync()
        {
            dotNetObjectRef ??= JSRunner.CreateDotNetObjectRef( new BreakpointActivatorAdapter( this ) );

            _ = JSRunner.RegisterBreakpointComponent( dotNetObjectRef, ElementId );

            await base.OnFirstAfterRenderAsync();
        }

        protected override void BuildClasses( ClassBuilder builder )
        {
            builder.Append( ClassProvider.Bar() );
            builder.Append( ClassProvider.BarBackground( Background ), Background != Background.None );
            builder.Append( ClassProvider.BarThemeContrast( ThemeContrast ), ThemeContrast != ThemeContrast.None );
            builder.Append( ClassProvider.BarBreakpoint( Breakpoint ), Breakpoint != Breakpoint.None );
            builder.Append( ClassProvider.FlexAlignment( Alignment ), Alignment != Alignment.None );
            builder.Append( ClassProvider.BarCollapsed( CurrentMode, CollapseMode ), !Visible );
            builder.Append( ClassProvider.BarMode( CurrentMode ) );

            base.BuildClasses( builder );
        }

        internal void Toggle()
        {
            Visible = !Visible;

            StateHasChanged();
        }

        public Task OnBreakpoint( bool broken )
        {
            if ( lastBrokenState == broken )
                return Task.CompletedTask;

            lastBrokenState = broken;
            Visible = !lastBrokenState;
            
            StateHasChanged();

            return Task.CompletedTask;
        }

        protected override void Dispose( bool disposing )
        {
            if ( disposing )
            {
                // make sure to unregister listener
                _ = JSRunner.UnregisterBreakpointComponent( this );

                JSRunner.DisposeDotNetObjectRef( dotNetObjectRef );
            }

            base.Dispose( disposing );
        }

        private void ToggleMode()
        {
            if ( CurrentMode == BarMode.Horizontal )
                return;

            CurrentMode = !Visible && collapseMode == BarCollapseMode.Small ?
                BarMode.VerticalSmall :
                initialMode;
        }

        #endregion

        #region Properties

        protected BarStore Store => store;

        protected string CollapseModeString => ClassProvider.ToBarCollapsedMode( CollapseMode );

        protected string ModeString => ClassProvider.ToBarMode( initialMode );

        protected BarMode CurrentMode
        {
            get => store.Mode;
            set
            {
                store.Mode = value;

                DirtyClasses();
            }
        }

        /// <summary>
        /// Controlls the state of toggler and the menu.
        /// </summary>
        [Parameter]
        public bool Visible
        {
            get => store.Visible;
            set
            {
                // prevent bar from calling the same code multiple times
                if ( value == store.Visible )
                    return;

                store.Visible = value;
                VisibleChanged.InvokeAsync( value );

                // Vertical bars need to manage their currentMode on Visible toggling
                ToggleMode();

                DirtyClasses();
            }
        }

        [Parameter] public EventCallback<bool> VisibleChanged { get; set; }

        /// <summary>
        /// Used for responsive collapsing.
        /// </summary>
        [Parameter]
        public Breakpoint Breakpoint
        {
            get => breakpoint;
            set
            {
                breakpoint = value;

                DirtyClasses();
            }
        }

        [Parameter]
        public ThemeContrast ThemeContrast
        {
            get => themeContrast;
            set
            {
                themeContrast = value;

                DirtyClasses();
            }
        }

        /// <summary>
        /// Defines the alignment within bar.
        /// </summary>
        [Parameter]
        public Alignment Alignment
        {
            get => alignment;
            set
            {
                alignment = value;

                DirtyClasses();
            }
        }

        /// <summary>
        /// Gets or sets the bar background color.
        /// </summary>
        [Parameter]
        public Background Background
        {
            get => background;
            set
            {
                background = value;

                DirtyClasses();
            }
        }

        /// <summary>
        /// Defines the Orientation for the bar. Vertical is required when using inside Sidebar.
        /// </summary>
        [Parameter]
        public BarMode Mode
        {
            get => initialMode;
            set
            {
                if ( initialMode == value )
                    return;

                store.Mode = value;
                initialMode = value;

                DirtyClasses();
            }
        }

        [Parameter]
        public BarCollapseMode CollapseMode
        {
            get => collapseMode;
            set
            {
                collapseMode = value;

                DirtyClasses();
            }
        }

        [Parameter] public RenderFragment ChildContent { get; set; }

        #endregion
    }
}
