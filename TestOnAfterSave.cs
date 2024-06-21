ClassMethod TestOnAfterSave() As %Status
{
    #dim obj as FLC.Data.Gp = ##class(FLC.Data.Gp).%New()
    #dim status as %Status

    // Set initial values
    set obj.Count = 0
    set obj.CustomerStatus = "Active"
    set obj.SupplierStatus = "Pending"
    set obj.RealEstateRelevant = "No"
    
    // Save initial state to have a reference point
    set status = obj.%Save()
    if '($$$ISOK(status)) {
        write "Failed to save initial object state", !
        quit status
    }

    // Simulate changes that should trigger onAfterSave
    set obj.CustomerStatus = "Inactive"
    set obj.SupplierStatus = "Approved"
    set obj.RealEstateRelevant = "Yes"

    // Call onAfterSave
    set status = ##class(FLC.Data.Gp).onAfterSave(obj)
    if '($$$ISOK(status)) {
        write "Failed to execute onAfterSave", !
        quit status
    }

    // Reload the object to verify changes
    set obj = ##class(FLC.Data.Gp).%OpenId(obj.%Id())

    // Verify Count field increment
    if obj.Count '= 1 {
        write "Count field not incremented correctly", !
        quit $$$ERROR($$$GeneralError, "Count field verification failed")
    }

    // Verify Note field
    if obj.Note '= "Inactive | Approved" {
        write "Note field not updated correctly", !
        quit $$$ERROR($$$GeneralError, "Note field verification failed")
    }

    // Verify subordinate records creation
    set regAdressenExists = ..CheckSubordinateExists(obj, "RegAdressen")
    set regPersonenExists = ..CheckSubordinateExists(obj, "RegPersonen")
    set regCurrentYearExists = ..CheckSubordinateExists(obj, "Reg"_$ZDATE($HOROLOG, "Y"))
    set projectOverviewExists = ..CheckSubordinateExists(obj, "ProjektUebersicht")

    if 'regAdressenExists {
        write "RegAdressen record not created", !
        quit $$$ERROR($$$GeneralError, "RegAdressen verification failed")
    }

    if 'regPersonenExists {
        write "RegPersonen record not created", !
        quit $$$ERROR($$$GeneralError, "RegPersonen verification failed")
    }

    if 'regCurrentYearExists {
        write "Current year register not created", !
        quit $$$ERROR($$$GeneralError, "Current year register verification failed")
    }

    if 'projectOverviewExists {
        write "ProjectOverview not created despite RealEstateRelevant being Yes", !
        quit $$$ERROR($$$GeneralError, "ProjectOverview verification failed")
    }

    write "All tests passed successfully", !
    quit $$$OK
}

ClassMethod CheckSubordinateExists(ByRef obj As FLC.Data.Gp, className As %String) As %Boolean
{
    #dim children as Filero.DataType.ListOfObjects = obj.getChildren(className)
    #dim cnt as Filero.DataType.Integer = children.Count()
    quit (cnt > 0)
}
