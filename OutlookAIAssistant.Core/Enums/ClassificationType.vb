Namespace OutlookAIAssistant.Core.Enums

    ''' <summary>
    ''' Defines the classification types for email categorization by the AI engine.
    ''' </summary>
    Public Enum ClassificationType
        ''' <summary>Could not be classified.</summary>
        Unknown = 0

        ''' <summary>Work-related email.</summary>
        Work = 1

        ''' <summary>Personal email.</summary>
        Personal = 2

        ''' <summary>Newsletter or subscription.</summary>
        Newsletter = 3

        ''' <summary>Marketing or promotional email.</summary>
        Marketing = 4

        ''' <summary>Social media notification.</summary>
        Social = 5

        ''' <summary>Meeting invitation or calendar-related.</summary>
        Meeting = 6

        ''' <summary>Task or action item request.</summary>
        Task = 7

        ''' <summary>Customer support or service request.</summary>
        Support = 8

        ''' <summary>Invoice, billing, or financial document.</summary>
        Finance = 9

        ''' <summary>Shipping or delivery notification.</summary>
        Shipping = 10

        ''' <summary>Security alert or account notification.</summary>
        Security = 11

        ''' <summary>Automated system notification.</summary>
        Notification = 12

        ''' <summary>Important or flagged as priority.</summary>
        Important = 13

        ''' <summary>Spam or junk email.</summary>
        Spam = 14

        ''' <summary>Internal company communication.</summary>
        Internal = 15

        ''' <summary>Project-related correspondence.</summary>
        Project = 16
    End Enum

End Namespace