using System.Collections.Generic;
using System.Linq;
using DigDig2.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DigDig2.UINavigation
{
    public class UserInterfaceNavigator : MonoBehaviour
    {
        [SerializeField] private string initialNavigationUri = "/";
        [Space(20)]
        [SerializeField] private string closedElementClass = "navigation-closed";
        [SerializeField] private string descendantOpenElementClass = "navigation-descendant-open";

        public string NavigationUri { get => navigationUri; set => NavigateTo(value); }
        private string navigationUri = "/";
        public NavigationNode Hierarchy { get => hierarchy; set { hierarchy = value; NavigateTo(initialNavigationUri, true); } }
        private NavigationNode hierarchy;

        

        public void NavigateTo(string uri, bool forceRefresh = false)
        {
            if (navigationUri != uri || forceRefresh)
            {
                navigationUri = uri;
                CloseNode(hierarchy);

                List<string> splitUri = uri.Split("/").ToList();
                if (uri == "/") splitUri.RemoveAt(1);
                splitUri[0] = hierarchy.name;

                Dictionary<string, Dictionary<string, string>> arguments = new();
                for (int index = 0; index < splitUri.Count; index++)
                {
                    string uriPart = splitUri[index];
                    if (!uriPart.Contains("?")) continue;

                    List<string> splitUriPart = uriPart.Split("?").ToList();
                    string partName = splitUriPart[0];
                    splitUri[index] = partName;
                    List<string> splitArguments = new();
                    if (splitUriPart[1].Contains(","))
                    {
                        splitArguments = splitUriPart[1].Split(",").ToList();
                    }
                    else
                    {
                        splitArguments.Add(splitUriPart[1]);
                    }

                    Dictionary<string, string> argumentValues = new();
                    foreach (string combinedArgument in splitArguments)
                    {
                        List<string> splitArgument = combinedArgument.Split("=").ToList();
                        argumentValues.Add(splitArgument[0], splitArgument[1]);
                    }
                    
                    arguments.Add(partName, argumentValues);
                }

                Debug.Log($"Navigating to: {uri}");
                OpenNode(hierarchy, splitUri, arguments);
            }
        }
        public void NavigateBack()
        {
            if (navigationUri == "/") return;

            List<string> splitUri = navigationUri.Split("/").ToList();
            splitUri.RemoveAt(splitUri.Count - 1);
            string newUri = string.Join("/", splitUri);
            if (newUri == string.Empty) newUri = "/";
            NavigateTo(newUri);
        }

        private void CloseNode(NavigationNode node)
        {
            node.element.AddToClassList(closedElementClass);
            if (node.inputElement != null) node.inputElement.enabledSelf = false;

            if (node.children != null)
            {
                foreach (NavigationNode child in node.children)
                {
                    CloseNode(child);
                }
            }
        }
        private void OpenNode(NavigationNode node, List<string> splitUri, Dictionary<string, Dictionary<string, string>> arguments, int splitIndex = 0)
        {
            if (splitIndex >= splitUri.Count) return;
            if (node.name == splitUri[splitIndex])
            {
                node.element.RemoveFromClassList(closedElementClass);
                
                if (splitIndex >= splitUri.Count - 1)
                {
                    if (node.openable == false) { NavigateBack(); return; }

                    node.element.RemoveFromClassList(descendantOpenElementClass);
                    if (node.inputElement != null)
                    {
                        node.inputElement.enabledSelf = true;
                        node.inputElement.Focus();
                    }
                    
                    NavigatorOpenedEvent openedEvent = new NavigatorOpenedEvent();
                    openedEvent.nodeName = node.name;
                    openedEvent.arguments = arguments;
                    openedEvent.target = node.element;
                    node.element.SendEvent(openedEvent);
                }
                else
                {
                    node.element.AddToClassList(descendantOpenElementClass);
                    if (node.inputElement != null) node.inputElement.enabledSelf = false;
                    
                    NavigatorDescendantOpenedEvent descendantOpenedEvent = new NavigatorDescendantOpenedEvent();
                    descendantOpenedEvent.nodeName = node.name;
                    descendantOpenedEvent.arguments = arguments;
                    descendantOpenedEvent.target = node.element;
                    node.element.SendEvent(descendantOpenedEvent);
                }

                if (node.children != null)
                {
                    splitIndex++;
                    foreach (NavigationNode child in node.children)
                    {
                        OpenNode(child, splitUri, arguments, splitIndex);
                    }
                }
            }
        }
    }

    public class NavigationNode
    {
        public string name;
        public VisualElement element;
        public VisualElement inputElement;
        public bool openable;
        public List<NavigationNode> children;

        public NavigationNode(string name, VisualElement element, VisualElement inputElement = null, bool openable = true, List<NavigationNode> children = null)
        {
            this.name = name;
            this.element = element;
            this.inputElement = inputElement;
            this.openable = openable;
            this.children = children;
        }
    }
}