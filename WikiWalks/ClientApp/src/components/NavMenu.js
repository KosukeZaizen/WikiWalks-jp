import React from 'react';
import { Container, Navbar, NavbarBrand, Button } from 'reactstrap';
import { Link } from 'react-router-dom';
import './NavMenu.css';

export default function NavMenu() {
    return (
        <header>
            <Navbar className="navbar-expand-sm navbar-toggleable-sm border-bottom box-shadow mb-3" light >
                <Container>
                    <NavbarBrand tag={Link} to="/" style={{ fontWeight: "bold" }}>うぃき忍者</NavbarBrand>
                    <Button
                        color="primary"
                        style={{ fontWeight: "bold" }}
                        href="https://wiki.lingual-ninja.com"
                        target="_blank"
                        rel="noopener noreferrer"
                        hreflang="en"
                    >English</Button>
                </Container>
            </Navbar>
        </header >
    );
}
