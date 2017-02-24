namespace Firefly.Repository
{
    public enum Errors
    {
        // General
        FieldMissing,
        InvalidArgument,
        MultipleErrors,
        HigherRoleManipulation,
        InvalidOperation,
        Conflict,
        FormErrors,
        Forbidden,

        // User profile
        EmailRequired,
        UserNoLongerExists,

        // Order
        OrderEmpty,
        CannotFinishOrder,
        CustomerEmailMissing,

        // Cashdesk
        NoSubjectInCashdesk,
        SubjectNotSeller,

        // Other
        ImageRejected
    }
}