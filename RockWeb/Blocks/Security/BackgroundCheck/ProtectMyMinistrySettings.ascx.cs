﻿// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.UI;

using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using System.Data.SqlClient;
using Rock.Checkr.Constants;
using Rock.Checkr;

namespace RockWeb.Blocks.Security.BackgroundCheck
{
    [DisplayName( "Protect My Ministry Settings" )]
    [Category( "Security > Background Check" )]
    [Description( "Block for updating the settings used by the Protect My Ministry integration." )]

    public partial class ProtectMyMinistrySettings : Rock.Web.UI.RockBlock
    {
        private const string GET_STARTED_URL = "http://www.rockrms.com/Redirect/PMMSignup";
        private const string PROMOTION_IMAGE_URL = "https://rockrms.blob.core.windows.net/resources/pmm-integration/pmm-integration-banner.png";
        private const string TYPENAME_PREFIX = "PMM - ";

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gDefinedValues.DataKeyNames = new string[] { "Id" };
            gDefinedValues.Actions.ShowAdd = true;
            gDefinedValues.Actions.AddClick += gDefinedValues_Add;
            gDefinedValues.GridRebind += gDefinedValues_GridRebind;
            gDefinedValues.GridReorder += gDefinedValues_GridReorder;
            gDefinedValues.Actions.ShowAdd = true;
            gDefinedValues.IsDeleteEnabled = true;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            nbNotification.Visible = false;

            if ( !Page.IsPostBack )
            {
                ShowDetail();
            }
            else
            {
                ShowDialog();
            }
        }

        #endregion

        #region Events

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the lbSaveNew control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSaveNew_Click( object sender, EventArgs e )
        {
            if ( !string.IsNullOrWhiteSpace( tbUserNameNew.Text ) && !string.IsNullOrWhiteSpace( tbPasswordNew.Text ) )
            {
                using ( var rockContext = new RockContext() )
                {
                    var settings = GetSettings( rockContext );
                    SetSettingValue( rockContext, settings, "UserName", tbUserNameNew.Text );
                    SetSettingValue( rockContext, settings, "Password", tbPasswordNew.Text, true );

                    string defaultReturnUrl = string.Format( "{0}Webhooks/ProtectMyMinistry.ashx",
                        CacheGlobalAttributes.Value( "PublicApplicationRoot" ).EnsureTrailingForwardslash() );
                    SetSettingValue( rockContext, settings, "ReturnURL", defaultReturnUrl );

                    rockContext.SaveChanges();

                    BackgroundCheckContainer.Instance.Refresh();

                    ShowView( settings );
                }
            }
            else
            {
                nbNotification.Text = "<p>Username and Password are both required.</p>";
                nbNotification.Visible = true;
            }
        }

        /// <summary>
        /// Handles the Click event of the lbEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEdit_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                ShowEdit( GetSettings( rockContext ) );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            if ( !string.IsNullOrWhiteSpace( tbUserName.Text ) && !string.IsNullOrWhiteSpace( tbPassword.Text ) )
            {
                using ( var rockContext = new RockContext() )
                {
                    var settings = GetSettings( rockContext );
                    SetSettingValue( rockContext, settings, "UserName", tbUserName.Text );
                    SetSettingValue( rockContext, settings, "Password", tbPassword.Text, true );
                    SetSettingValue( rockContext, settings, "ReturnURL", urlWebHook.Text );
                    SetSettingValue( rockContext, settings, "Active", cbActive.Checked.ToString() );
                    rockContext.SaveChanges();

                    BackgroundCheckContainer.Instance.Refresh();

                    ShowView( settings );
                }
            }
            else
            {
                nbNotification.Text = "<p>Username and Password are both required.</p>";
                nbNotification.Visible = true;
            }

        }

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                ShowView( GetSettings( rockContext ) );
            }
        }

        #endregion

        #region Package Grid Events

        /// <summary>
        /// Handles the GridRebind event of the gDefinedValues control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gDefinedValues_GridRebind( object sender, EventArgs e )
        {
            BindPackageGrid();
        }

        /// <summary>
        /// Handles the RowSelected event of the gDefinedValues control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gDefinedValues_RowSelected( object sender, RowEventArgs e )
        {
            ShowPackageEdit( e.RowKeyId );
        }

        /// <summary>
        /// Handles the GridReorder event of the gDefinedValues control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gDefinedValues_GridReorder( object sender, GridReorderEventArgs e )
        {
            var definedType = CacheDefinedType.Get( Rock.SystemGuid.DefinedType.BACKGROUND_CHECK_TYPES.AsGuid() );
            if ( definedType != null )
            {
                var changedIds = new List<int>();

                using ( var rockContext = new RockContext() )
                {
                    var definedValueService = new DefinedValueService( rockContext );
                    var definedValues = definedValueService.Queryable().Where( a => a.DefinedTypeId == definedType.Id ).Where( a => a.ForeignId == 1 ).OrderBy( a => a.Order ).ThenBy( a => a.Value );
                    changedIds = definedValueService.Reorder( definedValues.ToList(), e.OldIndex, e.NewIndex );
                    rockContext.SaveChanges();
                }

                CacheDefinedType.Remove( definedType.Id );
                foreach ( int id in changedIds )
                {
                    Rock.Cache.CacheDefinedValue.Remove( id );
                }
            }

            BindPackageGrid();
        }

        /// <summary>
        /// Handles the Add event of the gDefinedValues control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gDefinedValues_Add( object sender, EventArgs e )
        {
            ShowPackageEdit( 0 );
        }

        /// <summary>
        /// Handles the Delete event of the gDefinedValues control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gDefinedValues_Delete( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var definedValueService = new DefinedValueService( rockContext );
                var value = definedValueService.Get( e.RowKeyId );
                if ( value != null )
                {
                    string errorMessage;
                    if ( !definedValueService.CanDelete( value, out errorMessage ) )
                    {
                        mdGridWarningValues.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    definedValueService.Delete( value );
                    rockContext.SaveChanges();

                    CacheDefinedType.Remove( value.DefinedTypeId );
                    CacheDefinedValue.Remove( value.Id );
                }

                BindPackageGrid();
            }
        }

        protected void dlgPackage_SaveClick( object sender, EventArgs e )
        {
            int definedValueId = hfDefinedValueId.Value.AsInteger();

            var definedType = CacheDefinedType.Get( Rock.SystemGuid.DefinedType.BACKGROUND_CHECK_TYPES.AsGuid() );
            if ( definedType != null )
            {
                using ( var rockContext = new RockContext() )
                {
                    var service = new DefinedValueService( rockContext );

                    DefinedValue definedValue = null;
                    if ( !definedValueId.Equals( 0 ) )
                    {
                        definedValue = service.Get( definedValueId );
                    }

                    if ( definedValue == null )
                    {
                        definedValue = new DefinedValue();
                        definedValue.DefinedTypeId = definedType.Id;
                        service.Add( definedValue );
                    }

                    definedValue.Value = TYPENAME_PREFIX + tbTitle.Text;
                    definedValue.Description = tbDescription.Text;
                    rockContext.SaveChanges();

                    definedValue.LoadAttributes( rockContext );

                    Guid? dvJurisdicationCodeGuid = null;
                    int? dvJurisdictionCodeId = ddlMVRJurisdication.SelectedValueAsInt();
                    if ( dvJurisdictionCodeId.HasValue && dvJurisdictionCodeId.Value > 0 )
                    {
                        var dvJurisdicationCode = CacheDefinedValue.Get( dvJurisdictionCodeId.Value );
                        if ( dvJurisdicationCode != null )
                        {
                            dvJurisdicationCodeGuid = dvJurisdicationCode.Guid;
                        }
                    }

                    definedValue.SetAttributeValue( "PMMPackageName", tbPackageName.Text );
                    definedValue.SetAttributeValue( "DefaultCounty", tbDefaultCounty.Text );
                    definedValue.SetAttributeValue( "SendHomeCounty", cbSendCounty.Checked.ToString() );
                    definedValue.SetAttributeValue( "DefaultState", tbDefaultState.Text );
                    definedValue.SetAttributeValue( "SendHomeState", cbSendState.Checked.ToString() );
                    definedValue.SetAttributeValue( "MVRJurisdiction", dvJurisdicationCodeGuid.HasValue ? dvJurisdicationCodeGuid.Value.ToString() : string.Empty );
                    definedValue.SetAttributeValue( "SendHomeStateMVR", cbSendStateMVR.Checked.ToString() );
                    definedValue.SaveAttributeValues( rockContext );

                    CacheDefinedType.Remove( definedType.Id );
                    CacheDefinedValue.Remove( definedValue.Id );
                }
            }

            BindPackageGrid();
            HideDialog();
        }

        /// <summary>
        /// Handles the Click event of the btnDefault control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDefault_Click( object sender, EventArgs e )
        {
            var bioBlock = CacheBlock.Get( Rock.SystemGuid.Block.BIO.AsGuid() );
            List<Guid> workflowActionGuidList = bioBlock.GetAttributeValues( "WorkflowActions" ).AsGuidList();
            if ( workflowActionGuidList == null || workflowActionGuidList.Count == 0 )
            {
                // Add Checkr to Bio Workflow Actions
                bioBlock.SetAttributeValue( "WorkflowActions", Rock.SystemGuid.WorkflowType.PROTECTMYMINISTRY );
                ///BackgroundCheckContainer.Instance.Components
            }
            else
            {
                //var workflowActionValues = workflowActionValue.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
                Guid guid = Rock.SystemGuid.WorkflowType.PROTECTMYMINISTRY.AsGuid();
                if ( !workflowActionGuidList.Any( w => w == guid ) )
                {
                    // Add Checkr to Bio Workflow Actions
                    workflowActionGuidList.Add( guid );
                }

                // Remove PMM from Bio Workflow Actions
                guid = CheckrSystemGuid.CHECKR_WORKFLOW_TYPE.AsGuid();
                workflowActionGuidList.RemoveAll( w => w == guid );
                bioBlock.SetAttributeValue( "WorkflowActions", workflowActionGuidList.AsDelimited( "," ) );
                string pmmTypeName = ( typeof( Rock.Security.BackgroundCheck.ProtectMyMinistry ) ).FullName;
                var pmmComponent = BackgroundCheckContainer.Instance.Components.Values.FirstOrDefault(c => c.Value.TypeName == pmmTypeName );
                // pmmComponent.Value.GetAttributeValue( "Active" );
                pmmComponent.Value.SetAttributeValue( "Active", "True" );
                pmmComponent.Value.SaveAttributeValue( "Active" );
            }

            bioBlock.SaveAttributeValue( "WorkflowActions" );

            using ( var rockContext = new RockContext() )
            {
                WorkflowTypeService workflowTypeService = new WorkflowTypeService( rockContext );
                // Rename PMM Workflow
                var pmmWorkflowAction = workflowTypeService.Get( Rock.SystemGuid.WorkflowType.PROTECTMYMINISTRY.AsGuid() );
                pmmWorkflowAction.Name = "Background Check";

                var checkrWorkflowAction = workflowTypeService.Get( CheckrSystemGuid.CHECKR_WORKFLOW_TYPE.AsGuid() );
                // Rename Checkr Workflow
                checkrWorkflowAction.Name = CheckrConstants.CHECKR_WORKFLOW_TYPE_NAME;

                rockContext.SaveChanges();
            }

            ShowDetail();
        }

        #endregion

        #endregion

        #region Internal Methods
        /// <summary>
        /// Haves the workflow action.
        /// </summary>
        /// <param name="guidValue">The Guid value of the action.</param>
        /// <returns>True/False if the Workflow contains the action</returns>
        private bool HaveWorkflowAction( string guidValue )
        {
            // workflowType.IsAuthorized( Authorization.VIEW, CurrentPerson

            using ( var rockContext = new RockContext() )
            {
                BlockService blockService = new BlockService( rockContext );
                AttributeService attributeService = new AttributeService( rockContext );
                AttributeValueService attributeValueService = new AttributeValueService( rockContext );

                var block = blockService.Get( Rock.SystemGuid.Block.BIO.AsGuid() );

                var attribute = attributeService.Get( Rock.SystemGuid.Attribute.BIO_WORKFLOWACTION.AsGuid() );
                var attributeValue = attributeValueService.GetByAttributeIdAndEntityId( attribute.Id, block.Id );
                if ( attributeValue == null || string.IsNullOrWhiteSpace( attributeValue.Value ) )
                {
                    return false;
                }

                var workflowActionValues = attributeValue.Value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );
                Guid guid = guidValue.AsGuid();
                return workflowActionValues.Any( w => w.AsGuid() == guid );
            }
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="restUserId">The rest user identifier.</param>
        public void ShowDetail()
        {
            using ( var rockContext = new RockContext() )
            {
                var mvrJurisdicationCodes = CacheDefinedType.Get( Rock.SystemGuid.DefinedType.PROTECT_MY_MINISTRY_MVR_JURISDICTION_CODES.AsGuid() );
                if ( mvrJurisdicationCodes != null )
                {
                    ddlMVRJurisdication.BindToDefinedType( mvrJurisdicationCodes, true, true );
                }

                var settings = GetSettings( rockContext );
                if ( settings != null )
                {
                    string username = GetSettingValue( settings, "UserName" );
                    string password = GetSettingValue( settings, "Password" );
                    if ( !string.IsNullOrWhiteSpace( username ) ||
                        !string.IsNullOrWhiteSpace( password ) )
                    {
                        ShowView( settings );
                    }
                    else
                    {
                        ShowNew();
                    }
                }
                else
                {
                    ShowNew();
                }
            }
        }

        /// <summary>
        /// Shows the new.
        /// </summary>
        public void ShowNew()
        {
            hlActive.Visible = false;

            imgPromotion.ImageUrl = PROMOTION_IMAGE_URL;
            hlGetStarted.NavigateUrl = GET_STARTED_URL;

            tbUserNameNew.Text = string.Empty;
            tbPasswordNew.Text = string.Empty;

            pnlNew.Visible = true;
            pnlViewDetails.Visible = false;
            pnlEditDetails.Visible = false;
            pnlPackages.Visible = false;

            HideSecondaryBlocks( true );
        }

        /// <summary>
        /// Shows the view.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public void ShowView( List<AttributeValue> settings )
        {
            ShowHighlightLabels( settings );

            lUserName.Text = GetSettingValue( settings, "UserName" );
            lPassword.Text = "********";

            using ( var rockContext = new RockContext() )
            {
                var packages = new DefinedValueService( rockContext )
                    .GetByDefinedTypeGuid( Rock.SystemGuid.DefinedType.BACKGROUND_CHECK_TYPES.AsGuid() )
                    .Where( v => v.ForeignId == 1 )
                    .Select( v => v.Value.Substring( TYPENAME_PREFIX.Length) )
                    .ToList();
                lPackages.Text = packages.AsDelimited( "<br/>" );
            }

            nbSSLWarning.Visible = !GetSettingValue( settings, "ReturnURL" ).StartsWith( "https://" );
            nbSSLWarning.NotificationBoxType = NotificationBoxType.Warning;

            pnlNew.Visible = false;
            pnlViewDetails.Visible = true;
            pnlEditDetails.Visible = false;
            pnlPackages.Visible = false;

            HideSecondaryBlocks( false );

            if ( HaveWorkflowAction( Rock.SystemGuid.WorkflowType.PROTECTMYMINISTRY ) )
            {
                btnDefault.Visible = false;
            }
            else
            {
                btnDefault.Visible = true;
            }
        }

        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public void ShowEdit( List<AttributeValue> settings )
        {
            ShowHighlightLabels( settings );

            tbUserName.Text = GetSettingValue( settings, "UserName" );
            tbPassword.Text = GetSettingValue( settings, "Password", true );
            urlWebHook.Text = GetSettingValue( settings, "ReturnURL" );
            cbActive.Checked = GetSettingValue( settings, "Active" ).AsBoolean();

            BindPackageGrid();

            pnlNew.Visible = false;
            pnlViewDetails.Visible = false;
            pnlEditDetails.Visible = true;
            pnlPackages.Visible = true;

            HideSecondaryBlocks( true );
        }

        /// <summary>
        /// Binds the package grid.
        /// </summary>
        public void BindPackageGrid()
        {
            using ( var rockContext = new RockContext() )
            {
                var definedValues = new DefinedValueService( rockContext )
                    .GetByDefinedTypeGuid( Rock.SystemGuid.DefinedType.BACKGROUND_CHECK_TYPES.AsGuid() )
                    .Where( a => a.ForeignId == 1 )
                    .ToList();

                foreach( var definedValue in definedValues )
                {
                    definedValue.LoadAttributes( rockContext );
                }

                gDefinedValues.DataSource = definedValues.Select( v => new
                {
                    v.Id,
                    Value = v.Value.Substring( TYPENAME_PREFIX.Length ),
                    v.Description,
                    PackageName = v.GetAttributeValue( "PMMPackageName" ),
                    DefaultCounty = v.GetAttributeValue( "DefaultCounty" ),
                    SendAddressCounty = v.GetAttributeValue( "SendHomeCounty" ).AsBoolean(),
                    DefaultState = v.GetAttributeValue( "DefaultState" ),
                    SendAddressState = v.GetAttributeValue( "SendHomeState" ).AsBoolean(),
                    MVRJurisdication = v.GetAttributeValue("MVRJurisdiction"),
                    SendAddressStateMVR = v.GetAttributeValue( "SendHomeStateMVR" ).AsBoolean()
                } )
                .ToList();
                gDefinedValues.DataBind();
            }
        }

        /// <summary>
        /// Shows the package edit.
        /// </summary>
        /// <param name="definedValueId">The defined value identifier.</param>
        public void ShowPackageEdit( int definedValueId )
        {
            var definedType = CacheDefinedType.Get( Rock.SystemGuid.DefinedType.BACKGROUND_CHECK_TYPES.AsGuid() );
            if ( definedType != null )
            {
                DefinedValue definedValue = null;
                if ( !definedValueId.Equals( 0 ) )
                {
                    definedValue = new DefinedValueService( new RockContext() ).Get( definedValueId );
                }

                if ( definedValue != null )
                {
                    hfDefinedValueId.Value = definedValue.Id.ToString();
                    dlgPackage.Title = definedValue.Value.Substring( TYPENAME_PREFIX.Length );
                }
                else
                {
                    definedValue = new DefinedValue();
                    definedValue.DefinedTypeId = definedType.Id;
                    hfDefinedValueId.Value = string.Empty;
                    dlgPackage.Title = "New Package";
                }

                tbTitle.Text = definedValue.Value.Substring( TYPENAME_PREFIX.Length );
                tbDescription.Text = definedValue.Description;

                definedValue.LoadAttributes();

                ddlMVRJurisdication.SetValue( 0 );
                Guid? mvrJurisdicationGuid = definedValue.GetAttributeValue( "MVRJurisdiction" ).AsGuidOrNull();
                if ( mvrJurisdicationGuid.HasValue )
                {
                    var mvrJurisdication = CacheDefinedValue.Get( mvrJurisdicationGuid.Value );
                    if ( mvrJurisdication != null )
                    {
                        ddlMVRJurisdication.SetValue( mvrJurisdication.Id );
                    }
                }

                tbPackageName.Text = definedValue.GetAttributeValue( "PMMPackageName" );
                tbDefaultCounty.Text = definedValue.GetAttributeValue( "DefaultCounty" );
                cbSendCounty.Checked = definedValue.GetAttributeValue( "SendHomeCounty" ).AsBoolean();
                tbDefaultState.Text = definedValue.GetAttributeValue( "DefaultState" );
                cbSendState.Checked = definedValue.GetAttributeValue( "SendHomeState" ).AsBoolean();
                cbSendStateMVR.Checked = definedValue.GetAttributeValue( "SendHomeStateMVR" ).AsBoolean();

                ShowDialog( "Package" );
            }
        }

        /// <summary>
        /// Shows the highlight labels.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public void ShowHighlightLabels( List<AttributeValue> settings )
        {
            bool active = GetSettingValue( settings, "Active" ).AsBoolean();
            hlActive.LabelType = active ? LabelType.Success : LabelType.Danger;
            hlActive.Text = active ? "Active" : "Inactive";
            hlActive.Visible = true;
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="dialog">The dialog.</param>
        private void ShowDialog( string dialog )
        {
            hfActiveDialog.Value = dialog.ToUpper().Trim();
            ShowDialog();
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        private void ShowDialog()
        {
            switch ( hfActiveDialog.Value )
            {
                case "PACKAGE":
                    dlgPackage.Show();
                    break;
            }
        }

        /// <summary>
        /// Hides the dialog.
        /// </summary>
        private void HideDialog()
        {
            switch ( hfActiveDialog.Value )
            {
                case "PACKAGE":
                    dlgPackage.Hide();
                    break;
            }

            hfActiveDialog.Value = string.Empty;
        }

        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private List<AttributeValue> GetSettings( RockContext rockContext )
        {
            var pmmEntityType = CacheEntityType.Get( typeof( Rock.Security.BackgroundCheck.ProtectMyMinistry ) );
            if ( pmmEntityType != null )
            {
                var service = new AttributeValueService( rockContext );
                return service.Queryable( "Attribute" )
                    .Where( v => v.Attribute.EntityTypeId == pmmEntityType.Id )
                    .ToList();
            }

            return null;
        }

        /// <summary>
        /// Gets the setting value.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        private string GetSettingValue( List<AttributeValue> values, string key, bool encryptedValue = false )
        {
            string value = values
                .Where( v => v.AttributeKey == key )
                .Select( v => v.Value )
                .FirstOrDefault();
            if ( encryptedValue && !string.IsNullOrWhiteSpace( value ))
            {
                try { value = Encryption.DecryptString( value ); }
                catch { }
            }

            return value;
        }

        /// <summary>
        /// Sets the setting value.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="values">The values.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        private void SetSettingValue( RockContext rockContext, List<AttributeValue> values, string key, string value, bool encryptValue = false )
        {
            if ( encryptValue && !string.IsNullOrWhiteSpace( value ) )
            {
                try { value = Encryption.EncryptString( value ); }
                catch { }
            }

            var attributeValue = values
                .Where( v => v.AttributeKey == key )
                .FirstOrDefault();
            if ( attributeValue != null )
            {
                attributeValue.Value = value;
            }
            else
            {
                var pmmEntityType = CacheEntityType.Get( typeof( Rock.Security.BackgroundCheck.ProtectMyMinistry ) );
                if ( pmmEntityType != null )
                {
                    var attribute = new AttributeService( rockContext )
                        .Queryable()
                        .Where( a =>
                            a.EntityTypeId == pmmEntityType.Id &&
                            a.Key == key
                        )
                        .FirstOrDefault();

                    if ( attribute != null )
                    {
                        attributeValue = new AttributeValue();
                        new AttributeValueService( rockContext ).Add( attributeValue );
                        attributeValue.AttributeId = attribute.Id;
                        attributeValue.Value = value;
                        attributeValue.EntityId = 0;
                    }
                }
            }

        }

        #endregion

    }
}