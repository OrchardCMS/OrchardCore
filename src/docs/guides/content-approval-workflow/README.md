# Creating a simple Content Approval Workflow

In this guide you will create a simple Approval workflow.
We will setup a workflow that models the following process:

1. When a contributor creates or updates a content item, two things will happen:
    a. An email is sent to the site moderator containing a message that there's a new content item ready for review. The message will include an "Approve" and "Reject" link.
    b. The UserTask activity will add two buttons to the created content item: "Approve" and "Reject".
2. When the moderator approves the content item, it is published. The contributor receives an email with a link to the published content item.
3. When the moderator rejects the changes, the contributor receives an email with the sad news. The contributor can however update the content item, causing the workflow to execute again.

The complete workflow looks like this:

## What you will need

* A Orchard Core CMS installation created using the Blog recipe.
* Having an SMTP host is optional, but recommended for easier inspection of received email messages. Any SMTP host will work, but I will be using [Smtp4Dev](https://github.com/rnwood/smtp4dev). Alternatively, you can configure the SMTP feature to use a pickup directory.

## Enabling Features

Go the to Admin area and select *Configuration* --> *Features*, then enable the following features:

* Email
* HTTP Workflows Activities
* Workflows

## Configuring SMTP

1. Go to *Configuration* --> *Settings* --> *Smtp*
2. Enter `noreply@acme.net` as the *Sender email address*.
3. For the *Delivery method*, either select `Network` or `Specified pickup directory`. If you choose `Network`, make sure you have an SMTP host running listening at the specified portnumber. I'll be using port `2525` and `localhost` for the host name.

## Creating the Contributor user



## Summary

You just created an Orchard Core CMS powered blog engine.
